var firebaseConfig = {
    apiKey: "AIzaSyDZHVXLR_1imwVb2mCArnYpdbNx6XUPZBQ",
    authDomain: "isolaatti-b6641.firebaseapp.com",
    databaseURL: "https://isolaatti-b6641.firebaseio.com",
    projectId: "isolaatti-b6641",
    storageBucket: "isolaatti-b6641.appspot.com",
    messagingSenderId: "556033448926",
    appId: "1:556033448926:web:35759afc37d6a1585b1b4f",
    measurementId: "G-JQV4WX75RK"
};
// Initialize Firebase
firebase.initializeApp(firebaseConfig);

let infoContainerText = document.getElementById("info");

firebase.auth().onAuthStateChanged(function(user){
    user.getIdToken().then(function(token){
       console.log(token);
       // call /api/ValidateGoogleAccessToken and wait until I get response
        
        let formData = new FormData();
        formData.append("accessToken",token);
        
        let request = new XMLHttpRequest();
        request.open("post", "/api/ValidateGoogleAccessToken");
        request.onload = function() {
            if(request.status === 200){
                window.location = "/";
            }
        }
        request.send(formData);
    });
});