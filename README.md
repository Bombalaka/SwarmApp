# SwarmApp - Cloud-Native ASP.NET Core Application

A scalable ASP.NET Core 9.0 web application designed for deployment in Docker Swarm with AWS cloud integration. This application demonstrates modern cloud-native patterns with flexible storage backends and containerized deployment.

## 🚀 Features

- **Multi-Database Support**: Choose between MySQL, DynamoDB, or In-Memory storage
- **Cloud Storage**: AWS S3 integration for file uploads with local fallback
- **Docker Swarm Ready**: Production-ready container orchestration
- **AWS CloudFormation**: Infrastructure as Code deployment
- **Flexible Configuration**: Feature flags for different deployment scenarios
- **Scalable Architecture**: Designed for horizontal scaling

## 🏗️ Architecture

### Database Options
- **MySQL**: Traditional relational database with Entity Framework Core
- **DynamoDB**: AWS NoSQL database for serverless scaling
- **In-Memory**: Development and testing purposes

### Storage Options
- **AWS S3**: Cloud object storage for production
- **Local Storage**: File system storage for development

### Deployment Options
- **Docker Swarm**: Container orchestration for production
- **AWS CloudFormation**: Infrastructure provisioning
- **Local Development**: Direct ASP.NET Core execution

## 📋 Prerequisites

### Development
- .NET 9.0 SDK
- Docker Desktop
- AWS CLI (for cloud deployment)
- Visual Studio Code or Visual Studio

### Production Deployment
- Docker Swarm cluster
- AWS Account with appropriate permissions
- AWS CLI configured with credentials

## ⚙️ Configuration

Configure the application using feature flags in `appsettings.json`:

- `FeatureFlags:MySql` - Use MySQL database
- `FeatureFlags:DynamoDB` - Use AWS DynamoDB  
- `FeatureFlags:UseS3` - Use S3 for file uploads

Key environment variables for production:
- `AWS_REGION` - AWS region (e.g., eu-west-1)
- `S3_BUCKET` - S3 bucket name for uploads

## 🚦 Getting Started

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SwarmApp
   ```

2. **Configure the application**
   - Edit `app/appsettings.Development.json` for local settings
   - Set feature flags based on your preferred storage backend

3. **Run the application**
   ```bash
   cd app
   dotnet restore
   dotnet run
   ```

4. **Access the application**
   - Open your browser to `https://localhost:yourport`

### Docker Development in Local

1. **Build the Docker image**
   ```bash
   cd app/Infra
   docker build -t swarmapp:latest -f Dockerfile ..
   ```

2. **Run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

## 🚀 Production Deployment

### Production Deployment

1. **Deploy AWS infrastructure**
   ```bash
   cd app/Infra
   aws cloudformation create-stack \
     --template-body file://App-template.yaml \
     --stack-name swarmdemo \
     --capabilities CAPABILITY_NAMED_IAM \
     --region eu-west-1
   ```

2. **Build and push Docker image**
   ```bash
   cd app/Infra
   ./build-pushv1.sh v3
   ```

3. **Deploy to Docker Swarm server**
   ```bash
   # Copy deployment script to your server
   scp deploy-swarm1.sh user@your-server:/home/user/
   
   # SSH to server and deploy
   ssh user@your-server
   ./deploy-swarm1.sh v3
   ```

The scripts automatically:
- Get ECR repository URI from SSM parameters
- Build and push Docker image to ECR
- Configure Docker Swarm with proper settings
- Deploy with rolling updates and health checks

## 📁 Project Structure

```
SwarmApp/
├── app/                          # Main application
│   ├── Controllers/              # MVC Controllers
│   ├── Models/                   # Data models
│   ├── Views/                    # Razor views
│   ├── Repository/               # Data access layer
│   ├── Storage/                  # File storage abstractions
│   ├── Configuration/            # Configuration models
│   ├── Data/                     # Entity Framework context
│   ├── Infra/                    # Infrastructure files
│   │   ├── Dockerfile           # Container definition
│   │   ├── docker-compose.yml   # Local development
│   │   ├── App-template.yaml    # CloudFormation template
│   │   └── deploy-swarm1.sh     # Deployment script
│   └── wwwroot/                  # Static web assets
├── docker-compose.swarm.yml      # Production swarm config
└── README.md                     # This file
```

## 🔧 Key Components

### Controllers
- **HomeController**: Main landing page and privacy policy
- **PostsController**: CRUD operations for posts with image uploads

### Repository Pattern
- **IPostRepository**: Repository interface
- **InMemoryRepository**: In-memory implementation for development
- **PostSqlRepository**: MySQL/Entity Framework implementation
- **PostDynamoRepository**: DynamoDB implementation

### Storage Abstraction
- **IImageStorage**: Storage interface for file uploads
- **LocalImageStorage**: Local file system implementation
- **S3ImageStorage**: AWS S3 implementation

## 🛠️ Development

### Adding New Features

1. **Database Changes**: Update models in `Models/` and repositories in `Repository/`
2. **UI Changes**: Modify views in `Views/` and controllers in `Controllers/`
3. **Configuration**: Add new settings to `appsettings.json` and configuration classes

### Testing Different Backends

Modify the feature flags in `appsettings.Development.json`:

```json
{
  "FeatureFlags": {
    "MySql": true,      // Use MySQL
    "DynamoDB": false,  // Use DynamoDB
    "UseS3": false      // Use local storage
  }
}
```

## 🔍 Monitoring and Troubleshooting

### Logs
- **Application Logs**: Use `dotnet run` or `docker logs`
- **Swarm Logs**: Use `docker service logs swarmapp_swarmapp`
- **AWS Logs**: Check CloudWatch for DynamoDB and S3 operations

### Health Checks
- Application endpoint: `http://localhost/`
- Docker service status: `docker service ps swarmapp_swarmapp`

### Common Issues
1. **Database Connection**: Verify connection strings and AWS credentials
2. **S3 Access**: Check IAM permissions and bucket policies
3. **Container Issues**: Verify image build and registry access

## 📈 Scaling

### Horizontal Scaling
- Modify `replicas: 2` in `docker-compose.swarm.yml`
- DynamoDB scales automatically
- S3 handles concurrent uploads

### Performance Tuning
- Adjust container resource limits
- Configure DynamoDB read/write capacity
- Implement caching strategies

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with different configuration options
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For issues and questions:
- Check the troubleshooting section above
- Review AWS CloudFormation stack events
- Check Docker Swarm service logs
- Verify AWS IAM permissions

