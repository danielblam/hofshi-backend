import { ping, url } from "./utilities.js"

async function logIn(email, password) {
    const request = new Request(`${url}/Accounts/Login`, {
        method: "POST",
        headers: { 'Content-Type': 'application/json' },
        body: `{"email":"${email}","password":"${password}"}`
    })
    try {
        var response = await fetch(request)
    }
    catch {
        $(".fail-text").html("Couldn't connect to the server.")
    }
    let status = await response.status;
    switch (status) {
        case 200:
            $(".log-in-button").prop("disabled", true);
            $(".fail-text").html("&nbsp;&nbsp;&nbsp;")
            let json = await response.json()
            let token = json["token"]
            let role = json["user"]["role"]
            let teamName = json["teamName"]
            console.log(JSON.stringify(json["user"]))
            localStorage.setItem("token", token)
            localStorage.setItem("user", JSON.stringify(json["user"]))
            localStorage.setItem("teamName", teamName)
            redirect(role)
            break;
        case 401:
        case 404:
            $(".fail-text").html(await response.text())
            break
    }
}

function redirect(role) {
    console.log(role)
    switch (role) {
        case 1:
            window.location.href = "./user.html"
            break;
        case 10:
            window.location.href = "./admin.html"
            break;
        case 20:
            window.location.href = "./superadmin.html"
            break;
        default:
            $(".fail-text").html("Your account is... problematic medias? :(")
    }
}

async function tryPing(token) {
    const request = new Request(`${url}/Accounts/Ping`, {
        method: "GET",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    })
    try {
        var response = await fetch(request)
    }
    catch {
        $(".fail-text").html("Server is offline.")
    }
    let status = response.status
    return response.ok
}

$(document).ready(async function () {

    let token = localStorage.getItem("token")
    if (token != null) {
        if(await tryPing(token)) {
            let role = JSON.parse(localStorage.getItem("user")).role
            redirect(role)
        }
    }

    $(".log-in-button").click(function () {
        let email = $(".login-email").val()
        let password = $(".login-password").val()
        if (email == "" || password == "") {
            $(".fail-text").html("Missing username/password!")
            return
        }
        $(".fail-text").html("&nbsp;&nbsp;&nbsp;")
        logIn(email, password)
    })
});