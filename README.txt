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