

var events = require('events');
var eventemitter = new events.EventEmitter();
var db = require('../database/mongodatabase');

var gamemanager = require('../game_manager/gamemanager');
var loginmanager = require('../room_manager/loginmanager');
var database=null;

exports.initdatabase = function(){
    db.connect(function(err) {
        if (err) {
            console.log('Unable to connect to Mongo.');
            process.exit(1);
        }
        console.log('Connected to the DB.');
        database = db.get();
        loginmanager.initdatabase(database);
        gamemanager.initdatabase(database);
    });

};

exports.initsocket = function(socket,io) 
{
    loginmanager.setsocketio(io);
    loginmanager.addsocket(socket.id); 
    gamemanager.setsocketio(io);
    gamemanager.addsocket(socket.id);  
    
    socket.on('REQ_LOGIN', function(data)
    {
        console.log('----- LOGIN  ----- : ', data);
        loginmanager.LogIn(socket, data);
    });

    socket.on('REQ_LOGOUT', function(data)
    {
        console.log('----- LOGOUT ----- : ', data);
        loginmanager.LogOut(socket, data);        
    });

    // Register
    socket.on('REQ_REGISTER', function(data)
    {
        console.log('----- REGISTER ----- : ', data);
        loginmanager.Register(socket, data);        
    });    
    
    socket.on('REQ_UPGRADEEXP', function(data)
    {
        console.log('----- UPGRADEEXP ----- : ', data);
        loginmanager.UpgradeExp(socket, data);        
    });    

    socket.on('REQ_TOPPLAYERLIST', function(data)
    {
        console.log('----- REQ_TOPPLAYERLIST ----- : ', data);
        loginmanager.GetTopPlayerList(socket, data);        
    });  

    socket.on('REQ_INVITEREPLY', function(data)
    {
        console.log('----- REQ_INVITEREPLY ----- : ', data);
        loginmanager.InviteReply(socket, data);        
    }); 

    socket.on('REQ_ADDFRIEND', function(data)
    {
        console.log('----- REQ_ADDFRIEND ----- : ', data);
        loginmanager.ReqAddFriend(socket, data);        
    });  

    socket.on('REQ_ADDFRIENDREPLY', function(data)
    {
        console.log('----- REQ_ADDFRIENDREPLY ----- : ', data);
        loginmanager.AddFriendReply(socket, data);        
    }); 

    socket.on('REQ_REMOVEFRIEND', function(data)
    {
        console.log('----- REQ_REMOVEFRIEND ----- : ', data);
        loginmanager.DelFriend(socket, data);        
    });  
    socket.on('REQ_CHANGE_PASSWORD', function(data)
    {
        loginmanager.ChangePassword(socket, data);
        console.log('new Password : ', data);
    });

    // Get user list in the room
    socket.on('REQ_USERLIST_ROOM', function(data)
    {
        //gamemanager.GetUserListInRoom(data.roomid);
    });

    // disconnect
    socket.on('disconnect', function(){
        console.log("----- DISCONNECTED -----");
        gamemanager.OnDisconnect(socket);
    });
    socket.on('reconnect', (attemptNumber) => {
        console.log(attemptNumber);
      });
}