apt-get update
curl -sL https://deb.nodesource.com/setup_14.x | sudo -E bash -
sudo apt-get install -y nodejs
apt-get install mysql-server
sudo mysql_secure_installation utility
mysql
CREATE USER spotajoe@localhost;
CREATE DATABASE spotajoedb;
GRANT ALL PRIVILEGES ON spotajoedb.* To 'spotajoe'@'localhost' IDENTIFIED BY '*****';
ctrl+d
mysql -u spotajoe
CREATE DATABASE spotajoedb;
USE spotajoedb;
ctrl+d
npm install mysql
sudo node main.js //(need sudo to run on port 80)

Note that this in j-http-dispatcher.js line has a habit of breaking:
if(util.inspect(config) == "[Function (anonymous)]")


To run it:


sudo npm install -g forever
And then start your application with:
forever server.js
Or as a service:
forever start server.js
Forever restarts your app when it crashes or stops for some reason. To restrict restarts to 5 you could use:
forever -m5 server.js
To list all running processes:
forever list
Note the integer in the brackets and use it as following to stop a process:
forever stop 0
Restarting a running process goes:
forever restart 0
If you're working on your application file, you can use the -w parameter to restart automatically whenever your server.js file changes:
forever -w server.js