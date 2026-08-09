//pull từ nhánh develop về nhé ae, merge thì cũng chọn develop

B1: Check appsettings.json


{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MusicStream;User Id=sa;Password=sa123;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%",
    "Issuer": "MusicStreamAPI",
    "Audience": "MusicStreamClient"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  //music
  "CloudflareR2": {
    "AccountId": "3b5e1c530b50900683e7286b1ec7c4fc",
    "AccessKey": "bfe416a7f1eb558344f8974a4ab49dfb",
    "SecretKey": "aa97c9a5e8d6b7f6b160e4d2a31fd3003ad950f875b3c53b31e3853d53523f30",
    "BucketName": "music-data",
    "Region": "auto",
    "PublicUrl": "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev"
  },
  "AllowedHosts": "*"
}

B2: sửa Properties/launchSettings.json

{

  "profiles": {
    "MusicStream": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }

}

B3: Package Manager -> update-database

B4: Run để load data