# SigOps Metrics

This is the source code for the new SigOps Metrics site (http://new.sigopsmetrics.com)

## Appsettings configuration

#### 1. Create appsettings.json file in SigOpsMetrics.API 

#### 2. Paste the following into the file 

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "EPPlus": {
    "ExcelPackage": {
      "LicenseContext": "NonCommercial"
    }
  },
  "AppConfig": {
    "AWSAccessKey": "",
    "AWSSecretKey": "",
    "AWSBucketName": "",
    "DataPullKey": "",
    "CorridorsKey": "",
    "SmtpUsername": "",
    "SmtpPassword": "",
    "DatabaseName": "" 
  },
  "ConnectionStrings": {
    "Default": ""
  }
}
```

#### 3. Fill in the following sensitive information under AppConfig:
    - AWSAccessKey: AWS Access key ID (ie. "AKIAIOSFODNN7EXAMPLE")
    - AWSSecretKey: AWS Secret access key (ie. "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY")
    - AWSBucketName: AWS bucket name (ie. "docexamplebucket1")
    - DataPullKey: Self-generated key to protect DataPull endpoint
    - CorridorsKey: Name of corridors .xlsx file stored in the AWS bucket (ie. example.xlsx) 
    - SmtpUsername: Username of email to send from (ie. example@gmail.com) *Currently only gmail is supported
    - SmtpPassword: Password of email to send from
    - DatabaseName: Name of sql database (ie. myDatabase)
    
#### 4. Fill in the following sensitive information under ConnectionStrings:
    - Default: Connection string to SQL database (ie. "server=myServer;user=myUsername;password=myPassword;database=myDatabase")