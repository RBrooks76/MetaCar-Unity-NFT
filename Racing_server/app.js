const createError = require('http-errors');
const express = require('express');
const path = require('path');
const cookieParser = require('cookie-parser');
const logger = require('morgan');
const cors = require('cors');
const clientsocket = require('./sockets/clientsocket');
const bodyParser = require("body-parser");
const fs = require("fs");

const app = express();


// Configuring express to use body-parser
// as middle-ware
app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());


// Get request for root of the app
app.get("/", function (req, res) {
  // Sending index.html to the browser
  res.sendFile(__dirname + "/WebGL Builds/index.html");
  // res.sendFile(__dirname + "/views/index.html");
});
  

const options = {
  key: fs.readFileSync("server.key"),
  cert: fs.readFileSync("server.cert"),
};
var webServer = require('https').createServer(options, app);
var socketServer = require("https").createServer(options, app);

var io = require('socket.io')(socketServer,
  {
    transports: ['websocket'],
    allowUpgrades: true,
    pingInterval: 25000,
    pingTimeout: 60000,
    cors: {
      origin: "*",
    }
  });

// view engine setup
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'jade');

app.use(cors('*'));
app.use(express.static(path.join(__dirname, 'public')));

// catch 404 and forward to error handler
app.use(function(req, res, next) {
  next(createError(404));
});

// error handler
app.use(function(err, req, res, next) {
  // set locals, only providing error in development
  res.locals.message = err.message;
  res.locals.error = req.app.get('env') === 'development' ? err : {};

  // render the error page
  res.status(err.status || 500);
  res.render('error');
});

clientsocket.initdatabase();
io.sockets.on('connection', function(socket) {
  console.log("- One socket connected : ", socket.id);
  clientsocket.initsocket(socket,io);

});


module.exports = {app, webServer, socketServer};
