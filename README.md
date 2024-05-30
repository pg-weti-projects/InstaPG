# InstaPG

## RUN

1. Navigate to WcfServiceLibrary1 and run WcfServiceLibrary1.sln.
2. Navigate to InstaPGClient and run InstaPGClient.sln.
3. Create new reference in InstaPGClient project:
   1. Right click on the InstaPGClient project -> Add -> Service Reference.
   2. In **Address** put: http://localhost:8733/Design_Time_Addresses/ActiveUsersService/
   3. Click 'Go'. If service will be available in **Services** you can fill **Namespace** with ActiveUsersServiceReference .
4. If no error occurs you can run the InstaPGClient project.


## ERRORS

1. If you have problem with creating new reference you can remove Connected Services, obj and bin directory in InstaPGClient directory.