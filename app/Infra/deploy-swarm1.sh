#!/bin/bash
export MSYS_NO_PATHCONV=1
# Docker Swarm Deployment Script for SwarmApp
# Usage: ./deploy-swarm.sh [image_tag] [environment] [replicas]
# Example: ./deploy-swarm.sh latest production 3

set -e

# Get values from CloudFormation-created SSM parameters
echo "📡 Getting configuration from SSM..."
ECR_URI=$(aws ssm get-parameter --name "/swarmapp/ecr/repository-uri" --query "Parameter.Value" --output text)
ECR_REGISTRY=$(echo $ECR_URI | cut -d'/' -f1)
S3_BUCKET=$(aws ssm get-parameter --name "/swarmapp/s3/bucket-name" --query "Parameter.Value" --output text)
AWS_REGION="eu-west-1"

# Parameters 
IMAGE_NAME="swarmapp"
IMAGE_TAG=${1:-v3}
ENVIRONMENT=${2:-Production}
REPLICAS=${3:-2}

echo "🚀 Starting Docker Swarm Deployment..."
echo "📦 Image: ${ECR_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
echo "🔄 Replicas: ${REPLICAS}"
echo "🌍 Environment: ${ENVIRONMENT}"
echo "🌎 Region: ${AWS_REGION}"

# Check if Docker Swarm is initialized
if ! docker info | grep -q "Swarm: active"; then
    echo "🔧 Initializing Docker Swarm..."
    docker swarm init
fi

# Login to ECR
echo "🔐 Logging into ECR on manager node..."
aws ecr get-login-password --region ${AWS_REGION} | docker login --username AWS --password-stdin ${ECR_REGISTRY}


# Pull the latest image to make sure it exists
echo "📥 Pulling image from ECR..."
docker pull ${ECR_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG} || true

# Create Docker Compose for Swarm
cat > docker-compose.swarm.yml << EOF
version: '3.8'

services:
  swarmapp:
    image: ${ECR_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}
    deploy:
      replicas: ${REPLICAS}
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
      update_config:
        parallelism: 1
        delay: 10s
        failure_action: rollback
      rollback_config:
        parallelism: 1
        delay: 10s
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
        reservations:
          memory: 256M
          cpus: '0.25'
    ports:
      - "80:80"
    environment:
      - ASPNETCORE_URLS=http://+:80
      - ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
      - DetailedErrors=true
      - AWS_DEFAULT_REGION=${AWS_REGION}
      - AWS_REGION=${AWS_REGION}
      - DynamoDBRegion=${AWS_REGION}
      - FeatureFlags__MySql=false
      - FeatureFlags__DynamoDB=true
      - FeatureFlags__UseDynamoDB=true
      - FeatureFlags__UseS3=true
      - S3__BucketName=${S3_BUCKET}

      
    networks:
      - swarmapp-network

networks:
  swarmapp-network:
    driver: overlay
    attachable: true
EOF

# Deploy to Swarm
echo "🚀 Deploying to Docker Swarm..."
docker stack deploy --with-registry-auth -c docker-compose.swarm.yml swarmapp

# Wait for deployment
echo "⏳ Waiting for deployment to stabilize..."
sleep 15

# Check service status
echo "📊 Checking service status..."
docker service ls

# Show running containers
echo "📦 Running containers:"
docker service ps swarmapp_swarmapp

# Show service logs (last 50 lines)
echo "📋 Recent service logs:"
docker service logs --tail 50 swarmapp_swarmapp

echo "✅ Deployment completed!"
echo ""
echo "🔍 Next steps:"
echo "1. Find your EC2 public IP in AWS Console"
echo "2. Visit: http://YOUR-EC2-PUBLIC-IP"
echo "3. Monitor with: docker service ls"
echo "4. View logs with: docker service logs swarmapp_swarmapp"
echo "5. Scale up/down with: docker service scale swarmapp_swarmapp=NUMBER"
echo ""
echo "🎯 Useful commands:"
echo "   Check nodes: docker node ls"
echo "   Check where containers run: docker service ps swarmapp_swarmapp"
echo "   Remove stack: docker stack rm swarmApp"