#!/bin/bash

# Build and Push to ECR Script
# Usage: ./build-and-push.sh [tag]

set -e

# Configuration - UPDATE THESE VALUES
ECR_REGISTRY="146624863550.dkr.ecr.eu-west-1.amazonaws.com"
IMAGE_NAME="swarmapp"
IMAGE_TAG=${1:-latest}
AWS_REGION="eu-west-1"

echo "🚀 Building and pushing swarmapp to ECR..."
echo "📦 Image: ${ECR_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
echo "📍 Region: ${AWS_REGION}"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running!"
    exit 1
fi

# Check if AWS CLI is configured
if ! aws sts get-caller-identity > /dev/null 2>&1; then
    echo "❌ AWS CLI not configured!"
    echo "💡 Run: aws configure"
    exit 1
fi

# Build the Docker image
echo "🔨 Building Docker image..."
docker build -t ${IMAGE_NAME}:${IMAGE_TAG} -f app/Infra/Dockerfile .

# Tag for ECR
echo "🏷️  Tagging for ECR..."
docker tag ${IMAGE_NAME}:${IMAGE_TAG} ${ECR_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}

# Login to ECR
echo "🔐 Logging into ECR..."
aws ecr get-login-password --region ${AWS_REGION} | docker login --username AWS --password-stdin ${ECR_REGISTRY}

# Push to ECR
echo "🚀 Pushing to ECR..."
docker push ${ECR_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}

echo "✅ Successfully pushed to ECR!"
echo "📦 Image: ${ECR_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
echo "🚀 Ready for deployment!"

# Show next steps
echo ""
echo "📋 Next steps:"
echo "1. Update ECR_REGISTRY in this script with your actual ECR URL"
echo "2. Run: ./deploy-swarm.sh to deploy to Docker Swarm"

