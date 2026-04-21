import { getSelf, url } from "./utilities.js";

var roleNames = { 1: "User", 10: "Admin", 20: "Super Admin" }
var roleValues = { "User": 1, "Admin": 10, "Super Admin": 20 }
var roleSelect = ["User", "Admin", "Super Admin"]

var users;
var teams;
var viewActive = true;

function getToken() {
    let token = localStorage.getItem("token")
    if (token == null) {
        window.location.href = "./index.html"
    }
    return token
}

async function loadData() {
    let token = getToken()

    users = await getUsers(token)
    teams = await getTeams(token)
    let preparedData = prepareData(true)
    return preparedData
}

function prepareData(active) {
    let preparedData = []
    users.forEach((user) => {
        if (user.isActive == active) {
            preparedData.push({
                "firstname": user.firstName,
                "lastname": user.lastName,
                "email": user.email,
                "team": teams.find(team => team.teamId == user.teamId).teamName,
                "role": roleNames[user.role],
                "actions":
                    `<button class="btn p-1 edit-user">✏️</button>
                <button class="btn p-1 save-user" data-user-id="${user.userId}" style="display:none">💾</button>
                <button class="btn p-1 delete-user">${active ? "❌" : "↩️"}</button>
                `
            })
        }
    })
    return preparedData
}

async function getUsers(token) {
    const request = new Request(`${url}/Accounts/SuperAdmin/Get`, {
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
        alert("Server is down...")
    }
    let status = response.status;
    console.log(status)
    if (response.ok) {
        let json = response.json()
        return json
    }
    switch (status) {
        case 400: break; // indicates failed authorization, which realistically should never happen 
        case 401:
            window.location.href = "./index.html"
            return
    }
}

async function getTeams(token) {
    const request = new Request(`${url}/Info/Teams`, {
        method: "GET",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    })
    let response = await fetch(request)
    let status = response.status;
    if (response.ok) {
        let json = response.json()
        return json
    }
    else {
        throw Error("How did this happen???")
    }
}

function editUser(cells) {
    $(cells).eq(0).html(`<input type="text" class="form-control border" value="${$(cells).eq(0).html()}">`)
    $(cells).eq(1).html(`<input type="text" class="form-control border" value="${$(cells).eq(1).html()}">`)
    $(cells).eq(3).html(`<select class="form-control border">${teams.map(team => `<option>${team.teamName}</option>`)}</select>`)
    let userRole = $(cells).eq(4).html()
    $(cells).eq(4).html(`<select class="form-control border">${roleSelect.map(role => `<option>${role}</option>`)}</select>`)
    $(cells).eq(4).find("select").val(userRole)
}

async function saveUser(cells, userId) {
    let firstName = $(cells).eq(0).find("input").val()
    let lastName = $(cells).eq(1).find("input").val()
    let teamName = $(cells).eq(3).find("select").val()
    let roleName = $(cells).eq(4).find("select").val()

    const request = new Request(`${url}/Accounts/SuperAdmin/Update/${userId}`, {
        method: "PUT",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getToken()}`
        },
        body: `{
        "firstName":"${firstName}",
        "lastName":"${lastName}",
        "role":${roleValues[roleName]},
        "teamId":${teams.find(team => team.teamName == teamName).teamId}
        }`
    })
    let response = await fetch(request)
    let status = response.status
    if (response.ok) {
        $(cells).eq(0).html(firstName)
        $(cells).eq(1).html(lastName)
        $(cells).eq(3).html(teamName)
        $(cells).eq(4).html(roleName)
        let userIndex = users.indexOf(users.find(user => user.userId == userId))
        users[userIndex].firstName = firstName
        users[userIndex].lastName = lastName
        users[userIndex].role = roleValues[roleName]
        users[userIndex].teamId = teams.find(team => team.teamName == teamName).teamId
        return true
    }
}

async function addNewUser(user) {
    const request = new Request(`${url}/Accounts/SuperAdmin/Create`, {
        method: "POST",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getToken()}`
        },
        body: `{
        "firstName":"${user.firstName}",
        "lastName":"${user.lastName}",
        "email":"${user.email}",
        "password":"${user.password}",
        "role":${user.role},
        "teamId":${user.teamId}
        }`
    })
    let response = await fetch(request)
    let status = response.status
    if (response.ok) {
        return response.text()
    }
    else if (status == 409) {
        $(".add-user-modal-fail-text").html(`משתמש עם האימייל ${user.email} כבר קיים.`)
    }
    return false
}
async function setUserActivity(userId, isActive) {
    const request = new Request(`${url}/Accounts/SuperAdmin/Update/${userId}`, {
        method: "PUT",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getToken()}`
        },
        body: `{
        "isActive":${isActive}
        }`
    })
    let response = await fetch(request)
    let status = response.status
    if(response.ok) {
        return true
    }
}

function addNewUserRow(user, newUserId) {
    userTable.row.add({
        "firstname": user.firstName,
        "lastname": user.lastName,
        "email": user.email,
        "team": teams.find(team => team.teamId == user.teamId).teamName,
        "role": roleNames[user.role],
        "actions":
            `<button class="btn p-1 edit-user">✏️</button>
                <button class="btn p-1 save-user" data-user-id="${newUserId}" style="display:none">💾</button>
                <button class="btn p-1 delete-user">❌</button>
                `
    }).draw(false);
}

function resetUserModal() {
    $(".add-user-firstname").val("")
    $(".add-user-lastname").val("")
    $(".add-user-email").val("")
    $(".add-user-password").val("")
    $(".add-user-role-select").val("")
    $(".add-user-team-select").val("")
}

var userTable;
var self;

$(document).ready(async function () {

    self = getSelf()
    $(".account-header-name").html(`${self.user.email}`)

    let data = await loadData()
    userTable = new DataTable("#usertable", {
        data: data,
        columns: [
            { data: 'firstname' },
            { data: 'lastname' },
            { data: 'email' },
            { data: 'team' },
            { data: 'role' },
            { data: 'actions' }
        ]
    })

    $("#usertable_wrapper").addClass("position-relative")
    $("#usertable_wrapper").prepend(`
        <div class="text-end">
        <input id="active-filter" type="checkbox" class="p-1 ms-2 my-1 isactive-checkbox">
        <label class="p-1 my-1">משתמשים לא פעילים</label>
        </div>
        `)

    $('#usertable').on('click', '.edit-user', function () {
        var row = $(this).closest('tr')
        var cells = $(row).children()

        $(this).hide()
        $(this).siblings(".save-user").show()

        editUser(cells)
    });
    $('#usertable').on('click', '.save-user', async function () {
        var row = $(this).closest('tr')
        var cells = $(row).children()

        if (await saveUser(cells, Number($(this).attr("data-user-id")))) {
            $(this).hide()
            $(this).siblings(".edit-user").show()
        }
    });
    $('#usertable').on('click', '.delete-user', async function () {
        var row = $(this).closest('tr')
        var cells = $(row).children()
        var email = $(cells).eq(2).html()
        var userId = users.find(user => user.email == email).userId

        if(await setUserActivity(userId,!viewActive)) {
            users.find(user => user.userId == userId).isActive = !viewActive
            userTable.row(row).remove().draw();
        }
    })
    $(".add-user-button").click(function () {
        $(".add-user-team-select").html(teams.map(team => `<option>${team.teamName}</option>`))
        $(".add-user-team-select").val("")
        $(".add-user-role-select").val("")
        $(".add-user-modal").modal("show")
    })
    $(".add-user-modal-confirm").click(async function () {
        let missing = []
        let classes = ["firstname", "lastname", "email", "password", "team-select", "role-select"]
        let names = ["שם פרטי", "שם משפחה", "אימייל", "סיסמה", "צוות", "הרשאות"]

        for (let i = 0; i < 6; i++) {
            let value = $(`.add-user-${classes[i]}`).val()
            if (value == "" || value == null) missing.push(names[i])
        }

        if (missing.length > 0) {
            $(".add-user-modal-fail-text").html(`חסר: ${missing.join(", ")}`)
            return
        }
        $(".add-user-modal-fail-text").html("")

        let user = {}
        let fieldNames = ["firstName", "lastName", "email", "password"]

        for (let i = 0; i < 4; i++) {
            user[fieldNames[i]] = $(`.add-user-${classes[i]}`).val()
        }

        let teamName = $(`.add-user-team-select`).val()
        user["teamId"] = teams.find(team => team.teamName == teamName).teamId
        let roleName = $(`.add-user-role-select`).val()
        user["role"] = roleValues[roleName]

        console.log(user)

        let newUserId = await addNewUser(user)
        console.log(newUserId)
        if (newUserId != false && viewActive == true) {
            addNewUserRow(user, newUserId)
            resetUserModal()
            $(".add-user-modal").modal("hide")
        }
    })
    $(document).on("click", ".isactive-checkbox", function () {
        viewActive = !viewActive
        let newData = prepareData(viewActive)
        console.log(newData)
        userTable.clear()
        userTable.rows.add(newData)
        
        userTable.draw()
    })

    $(".account-header-logout").click(function () {
        console.log("a")
        localStorage.removeItem("token")
        localStorage.removeItem("user")
        window.location.href = "./index.html"
    })
})