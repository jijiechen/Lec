# Execute docker run -d -v (WorkDir):/lec-data -p 5000:5000 jijiechen/lecweb


FROM microsoft/dotnet:2.1.4-runtime

ENV ASPNETCORE_URLS http://*:5000
VOLUME /lec-data


COPY . /lec-app
WORKDIR /
ENTRYPOINT ["dotnet", "/lec-app/Lec.Web.dll"]