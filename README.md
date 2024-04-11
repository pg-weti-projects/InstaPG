# InstaPG

## RUN

1. Run **InstaPGService/InstaPGService.sln** ( you have to have Visual Studio ( preferred ) or Rider )
2. It is worth adding the option of running the service application (WCF) and the client application (WPF) simultaneously.
To do this, in **Visual Studio**, in **Solution Explorer** right-click on **Solution 'InstaPGService'** and enter properties.
In the Common **Properties/Startup Project** tab, select the **Multiple startup projects** option and set Action to 'Start'
for **InstaPGClient** and **InstaPGService**. After this operation, both applications will start automatically when you
run project from VS.


## DEPENDENCIES

InstaPGService --> WCF Service app ( broker for client apps )

InstaPGClient --> WPF Client app