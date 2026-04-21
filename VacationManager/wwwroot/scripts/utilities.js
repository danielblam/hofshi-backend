function getToken() {
    let token = localStorage.getItem("token")
}

const url = `/api`

async function ping(token) {
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
        return null
    }
    let status = response.status
    return response.ok
}

function repeat(value, length) {
    let out = []
    for (let i = 0; i < length; i++) {
        out.push(value)
    }
    return out
}

function getSelf() {
    let token = localStorage.getItem("token")
    if (token == null) {
        window.location.href = "./index.html"
    }
    let user = localStorage.getItem("user")
    let teamName = localStorage.getItem("teamName")
    return { token: token, user: JSON.parse(user), teamName: teamName }
}

function resetVacationDayInfo() {
    $(".vacation-days").html(`<div class="border p-2 rounded vacation-selection-info">
                            בחרו תאריכי התחלה וסיום, ואת סוג החופשה.
                        </div>`)
    $(".submit-request").addClass("disabled")
}
// converts a date string to a date object
function toDate(str) {
    let values = str.split("-").map(value => Number(value))
    let date = new Date(values[0], values[1] - 1, values[2])
    return date
}
function getIsraelBusinessDays(start, end) {
    let days = []
    for (let i = 0; ; i++) {
        let added = dateFns.addDays(start, i)
        if (!dateFns.isBefore(added, dateFns.addDays(end, 1))) {
            break
        }
        if (added.getDay() < 5) {
            days.push(added)
        }
    }
    return days
}

function resetVacationModal() {
    $(".request-vacation-modal").modal("hide");
    $(".vacation-start").val("")
    $(".vacation-end").val("")
    $(".vacation-type").val("")
    $(".vacation-user").val("")
    resetVacationDayInfo()
}

async function getEvents(token) {
    const request = new Request(`${url}/Events`, {
        method: "GET",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    })
    let response = await fetch(request)
    if (response.ok) {
        let json = await response.json()
        return prepareEventData(json)
    }
}
function prepareEventData(events) {
    return events.map(event => {
        event.startDate = toDate(event.startDate)
        event.endDate = toDate(event.endDate)
        return event
    })
}

function isEventDay(day, events) {
    let dayevents = []
    for (const event of events) {
        if (dateFns.isWithinInterval(day, {
            start: event.startDate,
            end: event.endDate
        })) {
            dayevents.push(event)
        }
    }
    return dayevents
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
    if (response.ok) {
        return await response.json()
    }
}





export {
    ping, repeat, getSelf, url,
    resetVacationDayInfo, toDate, getIsraelBusinessDays, resetVacationModal,
    getEvents, isEventDay, getTeams
}