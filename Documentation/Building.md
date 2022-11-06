# Build commands

Clear nuget cache:
```
dotnet nuget locals all --clear 
```

Build release version:
```
dotnet build --configuration Release  
```

Build nuget package:
```
dotnet pack -c Release
```