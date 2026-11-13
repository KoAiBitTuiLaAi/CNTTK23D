# EduSubmit

Small .NET 10 projects for the distributed applications assignment. The folder includes the BasicWebApi practice and the EduSubmit API/client project.

## Run

Start the API:

```powershell
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Start the client in a second terminal:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000 dtc245200427 https://github.com/dongtuan-creator/CNTTK23D
```

The client defaults to student code `dtc245200427` and repository `https://github.com/dongtuan-creator/CNTTK23D`; override them with command-line arguments when needed.

