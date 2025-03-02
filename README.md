# Assignment 3 & 4
in this repo contains the implementation for the task 3 and 4 that was given which are

- using rate limiting to limit request through an ip or api key
- using hangfire for relegating task to a background service and implementing a recurring cron job

# HOW TO RUN THE PROJECT
- install visual studio and clone this repo to your repository
- click on the soltion file( the file that ends with a .sln extension)
- once the project opens and you have a good internet connection the dependencies should be installed automatically
- another option for installing depencies is to go to the csproj file and install one after the other
- Navigate to the appsetting.json and make sure you put the right credentials for your sqlserver username and password

once the processes above has been done correctly then start the application by clicking on the green play button on visual studio,
all migrations should be applied and the program should run very fine.

# HOW TO TEST RATE LIMITING
once the program starts it should redirect you to the swagger documentation when you can call the weatherforecast endpoint
once your request exceeds 5 in a minute you should get a 429 response saying too many requests

# HOW TO TEST HANGFIRE IMPLEMENTATION
once the program starts it should redirect you to the swagger documentation when you can call the SENDMAIL endpoint
once that call is made the execution is relegated to a background service which queues the execution appropiately
also a simulated report generatore has been scheduled to run daily...you can find details of this implementation in the txt file that contains logs in the project
