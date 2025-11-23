#!/bin/bash
cd src/NetGPT.API
dotnet ef migrations add InitialCreate --project ../NetGPT.Infrastructure --startup-project .
dotnet ef database update --project ../NetGPT.Infrastructure --startup-project .
