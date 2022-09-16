const publicIp = require('public-ip');
var fs = require('fs');
var dateFormat = require("dateformat");
var database = null;
var serverip = '192.168.8.220';
//var serverip = '223.104.236.75';
var port = '16000';
var io;
var socketlist = [];

exports.initdatabase = function(db) {
    database = db;

    //setInterval(intervalFunc, 1500);

    (async () => {
        // console.log(await publicIp.v4());
        //serverip = await publicIp.v4();
        console.log(serverip);
        //=> '46.5.21.123'
     
        //console.log(await publicIp.v6());
        //=> 'fe80::200:f8ff:fe21:67cf'
    })();
};

function intervalFunc() {
  console.log('Cant stop me now!');
}


exports.addsocket = function (id)
{
    socketlist.push(id);
}
exports.setsocketio = function (socketio) {
    io = socketio;
};

exports.LogIn = function (socket,  userInfo) 
{
    var collection = database.collection('userdatas');
    var query = {userid: userInfo.userid};
    collection.findOne(query, function(err, result)
    {
        if(err)
            console.log(err);
        else
        {
            var mydata;
            if(result == null){
                mydata = {
                  result:'ERR_NOUSER'  
                };

                socket.emit('REQ_LOGIN_RESULT', mydata);
                return;
            }
            else
            {
                if(result.login_status == '1'){
                    mydata = {
                        result:'ERR_LOGINED'
                     };
                    socket.emit('REQ_LOGIN_RESULT', mydata);
                    return;
                }      
                if(result.password != userInfo.userpassword ){
                    mydata = {
                        result:'ERR_PASSWORD'  
                      };
                    socket.emit('REQ_LOGIN_RESULT', mydata);
                    return;
                }

                collection.updateOne(query,{$set:{connect:socket.id, status:1, login_status:'1'}},function(err) {
                    if(err) 
                        console.log(err);
                    //else
                    //console.log('- User socket_id:', socket.id);
                });             
                
                var mydata = {
                    result : 'success'
                    //points : result.points, 
                    //level : result.level,
                    //exp : result.exp
                }

                console.log('---' + result.username + ' s LOGIN INFO ---' , mydata);
            }

            socket.emit('REQ_LOGIN_RESULT', mydata);
        }
    });
}

exports.LogOut = function (socket,  data) 
{
    var collection = database.collection('userdatas');
    var query = {userid: data.userid};
    collection.findOne(query, function(err, result)
    {
        if(err)
            console.log(err);
        else
        {
            var mydata;
            if(result == null){
                console.log('logout failed ---');
            }
            else
            {
                collection.updateOne(query,{$set:{connect:socket.id, status:1, login_status:'0'}},function(err) {
                    if(err) throw err;
                    //else
                    //console.log('- User socket_id:', socket.id);
                });
            }
        }
    });
}


exports.Register = function (socket, data)
{
    var collection = database.collection('userdatas');
    var query = {userid: data.userid};
    collection.findOne(query, function(err, result)
    {
        if(err)
            console.log(err);
        else
        {
            var mydata;
            if(result == null)
            {
                // can regist new user
                let currentTime = new Date();
                let timel =  dateFormat(currentTime, "dddd mmmm dS yyyy h:MM:ss TT");  

                var user_data = {
                    userid : data.userid, 
                    password : data.password,
                    discordid : data.discordid,
                    wallet : data.wallet,
                    points : 10000, //signup_bonus
                    level : 1,
                    exp : 0, 
                    created_date : timel,

                    connect : socket.id,
                    winning_amount : 0,
                    status : 1,
                    login_status : "1"
                };
                
                collection.insertOne(user_data);

                var mydata = {
                    result : 'success'
                    //points : 10000, 
                    //level : 1,
                    //exp : 0,
                   // winning_amount : 0
                }

            }
            else
            {
                // already exits user
                mydata = {
                  result:'exits_user'
                };

            }

            console.log("---- REQ_REGISTER_RESULT : ", mydata);
            socket.emit('REQ_REGISTER_RESULT', mydata);
        }
    });
}

exports.UpgradeExp = function (socket,  userInfo) 
{
    var collection = database.collection('userdatas');
    var query = {userid: userInfo.userid};
    collection.findOne(query, function(err, result)
    {
        if(err)
            console.log(err);
        else
        {
            var mydata;
            if(result == null){
                mydata = {
                  result:'ERR_NOUSER'  
                };
            }
            else
            {
                var add_exp = 0;
                var cur_exp = result.exp;
                if (userInfo.rank == "1")
                {
                    if (userInfo.mapid == "1")
                        add_exp = 5;
                    else if (userInfo.mapid == "2")
                        add_exp = 10;
                }
                else
                {
                    add_exp = -2;
                }

                cur_exp += add_exp;

                collection.updateOne(query,{$set:{exp:cur_exp}},function(err) {
                    if(err) throw err;
                });

                mydata = {
                                result : 'success',
                                exp : cur_exp
                             };      
            }

            console.log("---- REQ_UPGRADEEXP_RESULT : ", mydata);
            socket.emit('REQ_UPGRADEEXP_RESULT', mydata);
        }
    });
}

exports.GetTopPlayerList = async function (socket)
{
    var collection = database.collection('userdatas');

    const users = await collection.find().limit(10).sort( { exp: -1 } ).toArray();

    const userExps = users.map(user => ({userid: user.userid, wallet: user.wallet, exp: user.exp}))
    
    var mydata = {
                                result : 'success',
                                userinfos : userExps
                             };

    console.log("---- REQ_UPGRADEEXP_RESULT : ", userExps);
    socket.emit('REQ_TOPPLAYERLIST_RESULT', userExps);
}

exports.SignUp = function (socket, data)
{
    let collection = database.collection('websettings');    
    let signup_bonus = 0;
    collection.findOne({}, function(err, result)
    {
        if(err)
            console.log(err);
        else
        {
            if(result != null)
            {
                signup_bonus = parseInt(result.signup_bonus);
                var collection = database.collection('userdatas');

                var randomnum1 = '' + Math.floor(100000 + Math.random() * 900000);
                var randomnum2 = '' + Math.floor(100000 + Math.random() * 900000);
                var randomnum = randomnum1 + randomnum2;
                    
                var email = data.useremail;

                var user_data = {
                    username : data.name,
                    userid : randomnum, 
                    password : data.password,
                    useremail : email,
                    points : 10000, //signup_bonus
                    level : 1,
                    exp : 0, 
                    friend_users : [],
                    created_date : timel,

                    connect : socket.id,
                    winning_amount : 0,
                    status : 1,
                    login_status:"1",
                };
                
                collection.insertOne(user_data);

                let websettings = database.collection('websettings');
                websettings.findOne({}, function(err, result)
                {
                    let webdata ;
                    if(err)
                        console.log(err);
                    if(result != null){
                        if(parseInt(result.activeplayer) >= 0)
                        {
                            websettings.updateOne({},{$set:{activeplayer:parseInt(result.activeplayer) + 1}},function(err) {
                                if(err) throw err; 
                            });
                        }
                    }
                });

                var mydata = {
                    result : 'success',
                    username : data.name,
                    userid : randomnum, 
                    useremail : data.useremail,
                    points : 10000, //signup_bonus
                    level : 1
                }
                //console.log("---- New Registered User : " + name);
                console.log("---- REQ_REGISTER_RESULT : ", mydata);                                    
                socket.emit('REQ_REGISTER_RESULT', mydata);
            }
        }
    }); 
}


exports.ChangePassword = function (socket,  data) 
{
    var collection = database.collection('userdatas');
    var query = {userphone: data.userphone};
    collection.findOne(query, function(err, result)
    {
        if(err)
            console.log(err);
        else
        {
            var mydata;
            if(result == null){
                mydata = {
                  result:'failed'  
                };
            }
            else
            {
                collection.updateOne(query,{$set:{password:data.newpassword}},function(err) {
                    if(err) throw err;
                    //else
                    //console.log('- User socket_id:', socket.id);
                });
                mydata = {
                                result : 'success',
                                newpassword : data.newpassword
                             };                             
            }            
            socket.emit('GET_CHANGEPASS_RESULT', mydata);
        }
    });
}


exports.Valid_UserID = function(socket, data)
{
    var collection = database.collection('userdatas');
    collection.find().toArray(function(err, docs)
    {
        if(err){
            throw err;
        }
        else
        {
            if(docs.length > 0)
            {
                var userdata = docs.filter(function (object) 
                {
                    return (object.userid == data.userid)
                });    

                if(userdata.length > 0)
                {
                    console.log('---- Already Exist Logined User -----');
                    var mydata = {
                        result:'failed'
                    }
                    socket.emit('REQ_CHECK_ID_RESULT', mydata);  
                }
                else
                {
                    console.log('success');
                    var mydata = {
                        result:'success'
                    }
                    socket.emit('REQ_CHECK_ID_RESULT', mydata);                     
                }
            }
            else
            {
                console.log('success');
                var mydata = {
                    result:'success'
                }
                socket.emit('REQ_CHECK_ID_RESULT', mydata);        
            }
        }
    });
}

exports.Get_Coins = function(data, socket)
{
    var collection = database.collection('userdatas');
    var query = {userphone : data.userphone};
    //console.log('userphone:  ' , data.userphone);
    collection.findOne(query, function(err, result)
    {
        if(err)
        {
            console.log(err);            
        }
        else
        {
            var mydata;
            if(result == null){
                mydata = {
                    result : "failed"
                }
            }
            else
            {
                mydata = {
                    result : 'success',
                    points : result.points,
                    winning_amount : result.winning_amount,
                }
            }
            //console.log('---- REQ_COIN_RESULT ----', mydata);
            socket.emit('REQ_COIN_RESULT', mydata);
        }
    });
}

exports.GetUserInfo = function(socket, userInfo)
{
    //console.log(userInfo.username);
    var collection = database.collection('userdatas');
    var query = {userid : userInfo.userid};
    collection.findOne(query, function(err, result)
    {
        if(err)
        {
            console.log(err);            
        }
        else
        {
            //console.log("- Login userinfo :");
            //console.log(result);
            var mydata;
            if(result == null){
                mydata = {
                    result : "failed"
                }
            }
            else
            {
                mydata = {
                    result : 'success',
                    username : result.username,                     
                    userid : result.userid,
                    photo: result.photo,
                    points : result.points, 
                    level : result.level
                }
            }
            socket.emit('GET_USERINFO_RESULT', mydata);
        }
    });
}

exports.GetWalletHistories = function (socket, data) {
    let transactions = database.collection('transactions');       
    transactions.find().toArray(function(err, docs){
        if(err){
            throw err;
        }
        else
        {
            if(docs.length > 0){
                var deposits = docs.filter(function (object) {
                    return (object.userid == data.userid)
                });
                
                if(deposits.length > 0)
                {
                    let collection = database.collection('withdraws');
                    collection.find().toArray(function(err, docs){
                        if(err){
                            throw err;
                        }
                        else
                        {
                            if(docs.length > 0){
                                var withdraws = docs.filter(function (object) {
                                    return (object.userid == data.userid)
                                });
                                
                                if(withdraws.length > 0)
                                {                                  
                                    //console.log('deposits : ', deposits);   
                                    //console.log('withdraws : ', withdraws); 
                                                       
                                    var mydata = {
                                        result : 'success',
                                        deposits : deposits,
                                        withdraws : withdraws,
                                        deposit_length : deposits.length,
                                        withdraw_length : withdraws.length                                   
                                    };
                                    socket.emit('REQ_WALLET_HIS_RESULT', mydata);  
                                }
                            }
                            else
                            {
                                var mydata = {
                                    result:'failed'
                                };
                                socket.emit('REQ_WALLET_HIS_RESULT', mydata);        
                            }
                        }
                    });
                }
            }
            else
            {
                var mydata = {
                    result:'failed'
                };
                socket.emit('REQ_WALLET_HIS_RESULT', mydata);        
            }
        }
    });
}

