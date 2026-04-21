import { HDate, gematriya, HebrewCalendar, Event } from 'https://cdn.jsdelivr.net/npm/@hebcal/core@6.0.6/+esm';
import {
    ping, repeat, getSelf, url,
    resetVacationDayInfo, toDate, getIsraelBusinessDays, resetVacationModal,
    getEvents, isEventDay,
    getTeams
} from "./utilities.js"

var self;

async function addNewVacation(userId, vacationRequest) {
    const request = new Request(`${url}/Vacations/Add/${userId}`, {
        method: "POST",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        },
        body: JSON.stringify(vacationRequest)
    })

    let response = await fetch(request)
    if (response.ok) return true
    else {
        console.log(await response.text())
        return false
    }
}
async function editVacation(vacationId, vacationRequest) {
    const request = new Request(`${url}/Vacations/Edit/${vacationId}`, {
        method: "PUT",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        },
        body: JSON.stringify(vacationRequest)
    })

    let response = await fetch(request)
    if (response.ok) {
        /*
        $(".request-vacation-modal").modal("hide");
        $(".vacation-start").val("")
        $(".vacation-end").val("")
        $(".vacation-type").val("")
        resetVacationDayInfo()
        */
        return true
    }
    else {
        console.log(await response.text())
        return false
    }
}
async function resolveVacation(vacationId, approveList) {
    const request = new Request(`${url}/Vacations/Resolve/${vacationId}`, {
        method: "PUT",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        },
        body: JSON.stringify(approveList)
    })

    let response = await fetch(request)
    if (response.ok) {
        return true
    }
    else {
        console.log(await response.text())
        return false
    }
}
async function addEvent(event) {
    const request = new Request(`${url}/Events`, {
        method: "POST",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        },
        body: JSON.stringify(event)
    })
    let response = await fetch(request)
    if (response.ok) return true
    else {
        console.log(await response.text())
        return false
    }
}
async function editEvent(eventId, event) {
    const request = new Request(`${url}/Events/${eventId}`, {
        method: "PUT",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        },
        body: JSON.stringify(event)
    })
    let response = await fetch(request)
    if (response.ok) return true
    else {
        console.log(await response.text())
        return false
    }
}
async function deleteEvent(eventId) {
    const request = new Request(`${url}/Events/${eventId}`, {
        method: "DELETE",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        }
    })
    let response = await fetch(request)
    if (response.ok) return true
    else {
        console.log(await response.text())
        return false
    }
}


async function getVacations() {

    const request = new Request(`${url}/Vacations/Team/${self.user.teamId}`, {
        method: "GET",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        }
    })
    let response = await fetch(request)
    let status = response.status
    if (response.ok) {
        let json = await response.json()
        return prepareVacationData(json)
    }
}
async function getUsers() {
    const request = new Request(`${url}/Accounts/Admin/Get`, {
        method: "GET",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        }
    })
    let response = await fetch(request)
    let status = response.status
    if (response.ok) {
        let json = await response.json()
        return json
    }
}
function prepareVacationData(json) { // converts all the date strings to date objects
    return json.map(vacation => {
        vacation.vacation.startDate = toDate(vacation.vacation.startDate)
        vacation.vacation.endDate = toDate(vacation.vacation.endDate)
        vacation.vacationDays = vacation.vacationDays.map(vacationDay => {
            vacationDay.date = toDate(vacationDay.date)
            return vacationDay
        })
        return vacation
    })
}
function getUser(userId) {
    return users.find(user => user.userId == userId)
}
function getVacation(vacationId) {
    return vacations.find(vacation => vacation.vacation.vacationId == vacationId)
}
async function deleteVacation(vacationId) {
    const request = new Request(`${url}/Vacations/Delete/${vacationId}`, {
        method: "DELETE",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        }
    })
    let response = await fetch(request)
    let status = response.status
    //console.log(await response.text())
    return response.ok
}
function renderCalendar(date) {
    let year = date.getFullYear()
    let month = date.getMonth()

    let startDay = dateFns.startOfWeek(dateFns.startOfMonth(new Date(year, month)))
    let endDay = dateFns.endOfWeek(dateFns.endOfMonth(new Date(year, month)))
    let difference = dateFns.differenceInCalendarWeeks(endDay, startDay)

    const holidays = HebrewCalendar.calendar({
        start: startDay,
        end: endDay,
        noRoshChodesh: true,
        noSpecialShabbat: true,
        noMinorFast: true
    });

    /*
    events.forEach(ev => {
        console.log(ev.greg(), ev.render('he'));
        console.log(ev)
    })*/

    let calendarHtml = ""
    for (let row = 0; row < 6; row++) {
        let rowHtml = ""
        for (let i = 0; i < 7; i++) {

            let added = dateFns.addDays(startDay, i + row * 7)
            let cellHtml = `<div class="d-flex flex-column">`
            let cellClasses = []

            let hebToday = new HDate(added)
            cellHtml += `<div>${added.getDate()} <span class="fw-normal heb-date">${hebToday.renderGematriya(false, true)}</span></div>`

            let cellHolidays = []
            holidays.forEach(holiday => {
                if (dateFns.isEqual(added, holiday.greg())) {
                    let emoji = holiday.emoji == undefined ? '' : holiday.emoji + ' '
                    cellHolidays.push(emoji + holiday.render('he'))
                }
            });
            let cellEvents = isEventDay(added, events)
            cellEvents.forEach(cellEvent => {
                cellHolidays.push(`<span class="text-danger ${cellEvent.teamId == self.user.teamId ? "fw-bold" : ""}">${cellEvent.name}</span>`)
            })

            cellHtml += `<div class="fw-normal heb-date text-primary">${cellHolidays.join(`<br>`)}</div>`



            let vacationDays = getVacationDays(added)
            cellHtml += `<div class="mt-auto${vacationDays.length > 2 ? ` day-vacations` : ''}">`

            if (vacationDays.length > 0) {
                vacationDays.forEach(function (vacationDay) {
                    let type = vacationDay.dayType
                    let typeName = vacationTypes[type - 1]
                    let user = getUser(getVacation(vacationDay.vacationId).vacation.userId)
                    let name = `${user.firstName} ${user.lastName}`
                    let status = vacationDay.status
                    let statusEmoji = ['❗', '⏳', ''][status + 1]
                    let statusClass = ['-pending', '-pending', ''][status + 1]
                    cellHtml += `<div class="hover-border day-status-${status + 1} p-0 px-1 m-0 fw-normal rounded" style="font-size:85%">${statusEmoji} ${name}</div>`
                })
            }
            else {
                cellHtml += `<div class="day-type-1 p-0 px-1 m-0 fw-normal rounded opacity-0">a</div>`
            }

            cellHtml += `</div></div>`

            if (added.getMonth() != month) {
                cellClasses.push("text-black")
                cellClasses.push("text-opacity-25")
            }
            if (added.getDay() >= 5) {
                cellClasses.push("text-primary-emphasis bg-secondary bg-opacity-25")
                if (added.getMonth() != month) cellClasses.push("opacity-50")
            }


            rowHtml += `<td class="${cellClasses.join(" ")}" data-date="${dateFns.format(added, "yyyy-MM-dd")}">${cellHtml}</td>`
        }
        calendarHtml += `<tr>${rowHtml}</tr>`
    }
    $(".calendar-body").html(calendarHtml)
}
function updateCalendarNavigationLabel(date) {
    let year = date.getFullYear()
    let month = date.getMonth()

    $(".calendar-navigation-label").html(`${monthNames[month]} ${year}`)
}
function updateStatsNavigationLabel(date) {
    let year = statsMonth.getFullYear()
    let month = statsMonth.getMonth()
    $(".stats-month-label").html(`${monthNames[month]} ${year}`)
}
function getVacationDays(date) {
    let days = []
    for (const vacation of vacations) {
        for (const vacationDay of vacation.vacationDays) {
            if (dateFns.isEqual(date, vacationDay.date)) {
                days.push(vacationDay)
            }
        }
    }
    return days
}
function getVacationDayByUserId(date, userId) {
    for (const vacation of vacations.filter(vacation => vacation.vacation.userId == userId)) {
        for (const vacationDay of vacation.vacationDays) {
            if (dateFns.isEqual(date, vacationDay.date)) {
                return vacationDay
            }
        }
    }
    return null
}
function buildVacationDayEditor(dates, types) {
    $(".vacation-days").html("")
    dates.forEach((date, index) => {
        $(".vacation-days").append(
            `<div class="vacation-day my-1 p-1 d-flex rounded fw-bold" data-day-type="${types[index]}">
                ${addOrEdit == "edit" ? `<input class="vacation-day-approval form-check-input vacation-check p-3 ms-2 m-auto" type="checkbox" checked>` : ``}
                <div class="col-3 p-1 ps-4">
                    ${date.getDate()}/${date.getMonth() + 1}/${date.getFullYear()} - יום ${weekDays[date.getDay()]}
                </div>
                <select class="form-control vacation-day-type">
                    <option>יום חופש</option>
                    <option>חצי יום חופש</option>
                    <option>יום בחירה</option>
                    <option>יום הצהרה</option>
                    <option>היעדרות צפויה</option>
                </select>
            </div>`)
    })
    types.forEach((type, index) => {
        $(".vacation-days").children().eq(index).find("select").val(vacationTypes[types[index] - 1])
    })
}
function buildVacationList() {
    $(".vacation-list-pending").html("")
    $(".vacation-list-approved").html("")
    vacations.forEach((vacation, index) => {
        let startDate = vacation.vacation.startDate
        let endDate = vacation.vacation.endDate
        let user = getUser(vacation.vacation.userId)
        let statusEmoji = ['&nbsp;❗', '⏳', '✅'][vacation.vacation.status + 1]
        let listToAppend = ['pending', 'pending', 'approved'][vacation.vacation.status + 1]
        $(`.vacation-list-${listToAppend}`).append(`
            <details ${openVacations[index] ? 'open' : ''} data-index="${index}" data-id="${vacation.vacation.vacationId}">
                <summary class="my-1">
                    ${user.firstName} ${user.lastName}: ${dateFns.format(endDate, "dd/MM/yy")} - ${dateFns.format(startDate, "dd/MM/yy")} <span class="fs-5">${statusEmoji}</span>
                </summary>
                
                ${vacation.vacationDays.map(vacationDay => {
            let date = vacationDay.date
            let type = vacationDay.dayType
            let dayStatusEmoji = ['❌', '', '✔️'][vacationDay.status + 1]
            return `<div class="row">
                                <div class="col-9">
                                <div class="vacation-list-day day-status-${vacationDay.status + 1} rounded pe-1 my-1">
                                ${dateFns.format(date, "dd/MM/yy")} - יום ${weekDays[date.getDay()]} - ${vacationTypes[type - 1]}
                                
                                </div>
                                </div>
                                <div class="col-3 fw-bold text-end p-0 my-1">${dayStatusEmoji}</div>
                                </div>`
        }).join("")}
                <button class="btn bg-secondary bg-opacity-50 my-2 delete-vacation-request">🗑</button>
                <button class="btn bg-secondary bg-opacity-50 my-2 resolve-vacation-request">📝</button>
            </details>
            `)
    })
}
function buildEventList() {
    $(".events-list").html("")
    $(".other-events-list").html("")
    events.forEach((event, index) => {
        let startDate = event.startDate
        let endDate = event.endDate
        let name = event.name
        let description = event.description
        let listToAppend = event.teamId == self.user.teamId ? "" : "other-"
        let otherTeam = event.teamId == self.user.teamId ? "" : `(${teams.find(team => team.teamId == event.teamId).teamName})<br>`
        $(`.${listToAppend}events-list`).append(`
            <details data-id="${event.eventId}">
                <summary class="my-1">
                    <span class="text-decoration-underline">${name}: ${dateFns.format(endDate, "dd/MM/yy")} - ${dateFns.format(startDate, "dd/MM/yy")}</span>
                    </summary>
                ${otherTeam}
                ${description}
                ${event.teamId == self.user.teamId ? `<div>
                    <button class="btn bg-secondary bg-opacity-50 my-2 delete-event">🗑</button>
                    <button class="btn bg-secondary bg-opacity-50 my-2 edit-event">✏️</button>
                    </div>` : ""}
            </details>
        `)
    })
}
function updateVacationModal() {
    switch (addOrEdit) {
        case "add":
            $(".vacation-user").show()
            $(".vacation-user-title").show()
            $(".submit-request").html("הוספה")
            $(".submit-request").addClass("disabled")
            break;
        case "edit":
            $(".vacation-user").hide()
            $(".vacation-user-title").hide()
            $(".submit-request").html("שמירה")
            $(".submit-request").removeClass("disabled")
            break;
    }
}
function vacationConflict() {

    $(".vacation-days").html(`<div class="border p-2 rounded vacation-selection-info vacation-conflict">
                            כבר יש למשתמש הנתון ימי חופשה בטווח הזה.
                        </div>`)
    $(".submit-request").addClass("disabled")
}
function drawChart(type) {
    let onlyMonthly = $(".stats-time-range").val() == "month"
    let labels = users.map(user => `${user.firstName} ${user.lastName}`)
    let data = [[], [], []]
    let colors = ["#ee8899", "#eecc77", "#88ee88"]
    let chartType = "bar"
    users.forEach(user => {
        let userVacations = vacations.filter(vacation => vacation.vacation.userId == user.userId)
        let userData = [0, 0, 0]
        userVacations.forEach(vacation => {
            vacation.vacationDays.forEach(vacationDay => {
                switch (Number(type)) {
                    case 0:
                        if (vacationDay.date.getMonth() == (statsMonth).getMonth()) {
                            userData[vacationDay.status + 1]++
                        }
                        break
                    case 1:
                        if (vacationDay.date.getFullYear() == (new Date()).getFullYear()) {
                            userData[vacationDay.status + 1]++
                        }
                        break
                }
            })
        })
        data[0].push(userData[0])
        data[1].push(userData[1])
        data[2].push(userData[2])
    })

    if (statsChart != undefined) statsChart.destroy()

    statsChart = new Chart($("#statistics-chart"), {
        type: "bar",
        data: {
            labels: labels,
            datasets: [
                {
                    backgroundColor: colors[2],
                    data: data[2],
                    label: "ימי חופש מאושרים"
                },
                {
                    backgroundColor: colors[1],
                    data: data[1],
                    label: "ימי חופש בהמתנה לאישור"
                },
                {
                    backgroundColor: colors[0],
                    data: data[0],
                    label: "ימי חופש לא מאושרים"
                }
            ]
        },
        options: {
            plugins: {
                legend: { display: true },
                title: { display: false }
            },
            maintainAspectRatio: false,
            scales: {
                x: {
                    stacked: true
                },
                y: {
                    stacked: true
                }
            }
        }
    })

}
function resetEventModal() {
    $(".add-event-name").val("")
    $(".add-event-description").val("")
    $(".event-start").val("")
    $(".event-end").val("")
    $(".event-is-public").val("0")
}

var monthNames = ['ינואר', 'פברואר', 'מרץ', 'אפריל', 'מאי', 'יוני', 'יולי', 'אוגוסט', 'ספטמבר', 'אוקטובר', 'נובמבר', 'דצמבר']
var weekDays = ['א', 'ב', 'ג', 'ד', 'ה', 'ו', 'ז']
var weekDayNames = ['ראשון', 'שני', 'שלישי', 'רביעי', 'חמישי', 'שישי', 'שבת']
var vacationTypes = ['יום חופש', 'חצי יום חופש', 'יום בחירה', 'יום הצהרה', 'היעדרות צפויה']
var currentDate = new Date()

var currentVacationDays = []
var dayTypes;
var startDate;
var endDate;

var teams;
var vacations;
var users;
var events;

var vacationToDeleteId;
var vacationToEdit;

var eventToDeleteId;
var eventToEdit;

var addOrEdit = "add"

var eventAddOrEdit = "add"
var eventStart;
var eventEnd;

var openVacations;

var statsChart;
var statsMonth = new Date()

$(document).ready(async function () {

    $(".vacation-type").val("")

    self = getSelf()
    let check = await ping(self.token)
    if (!check) window.location.href = "./index.html"

    $(".vacation-list-team-name").html(self.teamName)

    teams = await getTeams(self.token)
    vacations = await getVacations()
    users = await getUsers()
    events = await getEvents(self.token)

    openVacations = repeat(false, vacations.length)

    buildVacationList()
    buildEventList()

    renderCalendar(currentDate)
    updateCalendarNavigationLabel(currentDate)

    $(".account-header-name").html(`${self.user.email}`)

    $(".statistics-filter-select").val("")
    $(".stats-month-nav").hide()

    $(".previous-month").click(function () {
        currentDate = dateFns.subMonths(currentDate, 1)
        renderCalendar(currentDate)
        updateCalendarNavigationLabel(currentDate)
    })
    $(".next-month").click(function () {
        currentDate = dateFns.addMonths(currentDate, 1)
        renderCalendar(currentDate)
        updateCalendarNavigationLabel(currentDate)
    })
    $(".request-vacation-button").click(function () {
        addOrEdit = "add"

        $(".vacation-start").val("")
        $(".vacation-end").val("")
        $(".vacation-type").val("")
        resetVacationDayInfo()

        let fullNames = users.map(user => {
            return [user.firstName, user.lastName, user.userId]
        })

        fullNames.sort((a, b) => a[1].localeCompare(b[1]))

        $(".vacation-user").html(fullNames.map(name => {
            return `<option value="${name[2]}">${name[0]} ${name[1]}</option>`
        }))
        $(".vacation-user").val("")

        updateVacationModal()
        $(".request-vacation-modal").modal("show")
    })
    $(".vacation-parameter").change(function () {

        let start = $(".vacation-start").val()
        let end = $(".vacation-end").val()
        let type = Number($(".vacation-type").val())

        if (start == "" || end == "" || type == 0) {
            resetVacationDayInfo()
            return
        }
        startDate = toDate(start)
        endDate = toDate(end)
        if (dateFns.isBefore(endDate, startDate) || dateFns.differenceInDays(endDate, startDate) > 366) {
            resetVacationDayInfo()
            return
        }
        let vacationDays = getIsraelBusinessDays(startDate, endDate)
        currentVacationDays = vacationDays

        if ($(".vacation-user").val() != null || addOrEdit == "edit") {
            $(".submit-request").removeClass("disabled")
        }

        buildVacationDayEditor(vacationDays, repeat(type, vacationDays.length))
    })
    $(".vacation-user").change(function () {
        let start = $(".vacation-start").val()
        let end = $(".vacation-end").val()
        let type = $(".vacation-type").val()
        if (start == "" || end == "" || type == null) {
            return
        }
        $(".submit-request").removeClass("disabled")

        if ($(".vacation-conflict").length > 0) {
            buildVacationDayEditor(currentVacationDays, dayTypes)
        }
    })
    $(document).on("change", ".vacation-day select", function () {
        let type = $(this).val()
        let typeIndex = vacationTypes.indexOf(type) + 1
        $(this).parent().attr('data-day-type', typeIndex)

        dayTypes = []
        $.each($(".vacation-days").children(), function (index, day) {
            let type = Number($(day).attr("data-day-type"))
            dayTypes.push(type)
        })
    })

    $(".submit-request").click(async function () {
        dayTypes = []
        $.each($(".vacation-days").children(), function (index, day) {
            let type = Number($(day).attr("data-day-type"))
            dayTypes.push(type)
        })

        var userId = addOrEdit == "add" ? Number($(".vacation-user").val()) : vacationToEdit.vacation.userId;

        for (const vacationDay of currentVacationDays) {
            let vacationDayInfo = getVacationDayByUserId(vacationDay, userId)
            if (vacationDayInfo != null) {
                if (addOrEdit == "add") {
                    vacationConflict()
                    return
                }
                else if (vacationDayInfo.vacationId != vacationToEdit.vacation.vacationId) {
                    vacationConflict()
                    return
                }
            }
        }

        let vacationRequest = {}

        vacationRequest["startDate"] = dateFns.format(startDate, "yyyy-MM-dd")
        vacationRequest["endDate"] = dateFns.format(endDate, "yyyy-MM-dd")
        vacationRequest["vacationDays"] = currentVacationDays.map((day, index) => {
            return { "dayType": dayTypes[index], "date": dateFns.format(day, "yyyy-MM-dd") }
        })

        switch (addOrEdit) {
            case "add":
                if (await addNewVacation(userId, vacationRequest)) {
                    resetVacationModal()
                    vacations = await getVacations()
                    openVacations.push(false)
                    buildVacationList()
                    renderCalendar(currentDate)
                }
                break;
            case "edit":
                if (await editVacation(vacationToEdit.vacation.vacationId, vacationRequest) == false) {
                    return
                }
                let approveList = []
                $.each($(".vacation-day-approval"), function (_, checkbox) {
                    approveList.push($(checkbox).prop('checked'))
                })
                if (await resolveVacation(vacationToEdit.vacation.vacationId, approveList)) {
                    resetVacationModal()
                    vacations = await getVacations()
                    buildVacationList()
                    renderCalendar(currentDate)
                }
                break;
            default:
                alert("Your vacation request is... lost media? :(")
        }
    })

    $(document).on("click", ".delete-vacation-request", async function () {
        let index = Number($(this).closest('details').attr('data-index'))

        vacationToDeleteId = vacations[index].vacation.vacationId
        let userId = vacations[index].vacation.userId
        let user = getUser(userId)

        let startDate = dateFns.format(vacations[index].vacation.startDate, "dd/MM/yy")
        let endDate = dateFns.format(vacations[index].vacation.endDate, "dd/MM/yy")


        $(".delete-vacation-modal-name").html(`${user.firstName} ${user.lastName}`)
        $(".delete-vacation-modal-range").html(`${endDate} - ${startDate} ?`)
        $(".delete-vacation-modal").modal("show")
    })

    $(".delete-vacation-modal-confirm").click(async function () {
        let index = Number($(`[data-id="${vacationToDeleteId}"]`).attr('data-index'))

        let result = await deleteVacation(vacationToDeleteId)
        if (result) {
            vacations = await getVacations()
            openVacations.splice(index, 1)
            buildVacationList()
            renderCalendar(currentDate)
            $(".delete-vacation-modal").modal("hide")
        }
    })

    $(document).on("click", ".resolve-vacation-request", async function () {
        let index = Number($(this).closest('details').attr('data-index'))
        vacationToEdit = vacations[index]
        addOrEdit = "edit"

        $(".vacation-start").val(dateFns.format(vacationToEdit.vacation.startDate, "yyyy-MM-dd"))
        $(".vacation-end").val(dateFns.format(vacationToEdit.vacation.endDate, "yyyy-MM-dd"))

        let vacationDays = vacationToEdit.vacationDays.map(day => day.date)
        let vacationDayTypes = vacationToEdit.vacationDays.map(day => day.dayType)
        $(".vacation-type").val(vacationDayTypes[0])

        buildVacationDayEditor(vacationDays, vacationDayTypes)

        startDate = vacationToEdit.vacation.startDate
        endDate = vacationToEdit.vacation.endDate
        currentVacationDays = vacationDays

        updateVacationModal()
        $(".request-vacation-modal").modal("show")
    })

    $(".account-header-logout").click(function () {
        localStorage.removeItem("token")
        localStorage.removeItem("user")
        window.location.href = "./index.html"
    })

    $(document).on("click", "details", function () {
        setTimeout(() => {
            openVacations[Number($(this).attr('data-index'))] = $(this).prop('open')
        }, 0)
    })

    $(".statistics-button").click(function () {
        updateStatsNavigationLabel(statsMonth)
        $(".five-day-list").html("")
        users.forEach(user => {
            let used5days = vacations.some(vacation => vacation.vacation.userId == user.userId
                && vacation.vacationDays.length >= 5
                && vacation.vacation.startDate.getFullYear() == (new Date()).getFullYear()
                && vacation.vacation.endDate.getFullYear() == (new Date()).getFullYear())
            let name = `${user.firstName} ${user.lastName}`
            $(".five-day-list").append(`<div class="rounded px-2 py-1 m-2">${used5days ? "✔️" : "❌"} ${name}</div>`)
        })
        $(".statistics-modal").modal("show")
    })

    $('input[name="stats-filter"]').on('change', function () {
        let type = $(this).val()
        if (type == "0") {
            $(".stats-month-nav").show()
        }
        else { $(".stats-month-nav").hide() }
        drawChart(type)
    });

    $(".stats-previous-month").click(function () {
        statsMonth = dateFns.subMonths(statsMonth, 1)
        updateStatsNavigationLabel(statsMonth)
        drawChart("0")
    })
    $(".stats-next-month").click(function () {
        statsMonth = dateFns.addMonths(statsMonth, 1)
        updateStatsNavigationLabel(statsMonth)
        drawChart("0")
    })

    $(".add-event-button").click(function () {
        eventAddOrEdit = "add"
        $(".submit-event").addClass('disabled')
        $(".event-fail").hide()

        $(".submit-event").html("הופסה")
        $(".add-event-modal-title").html(`הוספת אירוע`)
        resetEventModal()
        $(".add-event-modal").modal("show")
    })

    $(".add-event-name").on("input", function () {
        $(".add-event-name-length").html($(this).val().length)
    })

    $(".add-event-description").on("input", function () {
        $(".add-event-description-length").html($(this).val().length)
    })

    $(".event-start").change(function () {
        eventStart = toDate($(this).val())
    })

    $(".event-end").change(function () {
        eventEnd = toDate($(this).val())
    })

    $(".event-parameter").change(function () {
        if (eventStart == undefined || eventEnd == undefined) return
        if (dateFns.differenceInDays(eventEnd, eventStart) < 0) {
            $(".submit-event").addClass("disabled")
            return
        }
        $(".submit-event").removeClass("disabled")
    })

    $(".submit-event").click(async function () {
        let eventName = $(".add-event-name").val()
        if (eventName.length == 0) {
            $(".event-fail").show()
            return
        }
        $(".event-fail").hide()

        let eventDescription = $(".add-event-description").val()
        let isPublic = $(".event-is-public").val() == "1" ? true : false

        let eventBody = {
            "name": eventName,
            "description": eventDescription,
            "startDate": $(".event-start").val(),
            "endDate": $(".event-end").val(),
            "isPublic": isPublic
        }

        switch (eventAddOrEdit) {
            case "add":
                if (await addEvent(eventBody)) {
                    events = await getEvents(self.token)
                    buildEventList()
                    renderCalendar(currentDate)
                    $(".add-event-modal").modal("hide")
                    resetEventModal()
                }
                break
            case "edit":
                let eventToEditId = eventToEdit.eventId
                if (await editEvent(eventToEditId, eventBody)) {
                    events = await getEvents(self.token)
                    buildEventList()
                    renderCalendar(currentDate)
                    $(".add-event-modal").modal("hide")
                    resetEventModal()
                }
                break
        }
    })

    $(document).on("click", ".delete-event", function () {
        eventToDeleteId = Number($(this).closest('details').attr('data-id'))
        let eventToDelete = events.find(event => event.eventId == eventToDeleteId)
        $(".delete-event-modal-name").html(eventToDelete.name)
        $(".delete-event-modal").modal("show")
    })

    $(".delete-event-modal-confirm").click(async function () {
        let result = await deleteEvent(eventToDeleteId)
        if (result) {
            events = events.filter(event => event.eventId != eventToDeleteId)
            buildEventList()
            renderCalendar(currentDate)
            $(".delete-event-modal").modal("hide")
        }
    })

    $(document).on("click", ".edit-event", function () {
        eventAddOrEdit = "edit"

        let eventId = Number($(this).closest('details').attr('data-id'))
        eventToEdit = events.find(event => event.eventId == eventId)

        $(".submit-event").html("שמירה")
        $(".add-event-modal-title").html(`עריכת אירוע (${eventToEdit.name})`)
        $(".event-fail").hide()
        $(".submit-event").removeClass("disabled")

        $(".add-event-name").val(eventToEdit.name)
        $(".add-event-description").val(eventToEdit.description)
        $(".event-start").val(dateFns.format(eventToEdit.startDate, "yyyy-MM-dd"))
        $(".event-end").val(dateFns.format(eventToEdit.endDate, "yyyy-MM-dd"))
        $(".event-is-public").val(eventToEdit.isPublic ? "1" : "0")

        $(".add-event-modal").modal("show")
    })

    $(document).on("click", "td", function () {
        let today = toDate($(this).attr("data-date"))

        let vacationDays = getVacationDays(today)
        if (vacationDays.length > 0) {
            $(".day-overview-modal-vacations").html("")
            vacationDays.forEach(vacationDay => {
                let user = getUser(getVacation(vacationDay.vacationId).vacation.userId)
                let name = `${user.firstName} ${user.lastName}`
                let status = vacationDay.status
                $(".day-overview-modal-vacations").append(`
                    <div class="my-3">
                    <span class="fw-bold day-status-${status + 1} p-1 px-2 rounded">${name}</span>
                    </div>
                    `)
            })
        }
        else {
            $(".day-overview-modal-vacations").html("אין מה להציג...")
        }


        const holidays = HebrewCalendar.calendar({ start: today, end: today, noRoshChodesh: true, noSpecialShabbat: true, noMinorFast: true });
        let dayEvents = isEventDay(today, events)
        if (dayEvents.concat(holidays).length > 0) {
            $(".day-overview-modal-holidays-events").html("")
            holidays.forEach(holiday => {
                $(".day-overview-modal-holidays-events").append(`
                    <div class="fw-bold text-primary my-2">
                        ${holiday.emoji} ${holiday.render("he")}
                    </div>
                    `)
            })
            dayEvents.forEach(event => {
                let teamName = event.teamId == self.user.teamId ? "" : ` (${teams.find(team => team.teamId == event.teamId).teamName})`
                $(".day-overview-modal-holidays-events").append(`
                    <div class="my-2">
                        <span class="text-danger fw-bold">${event.name}</span>${teamName}
                        <br>
                        ${event.description}
                    </div>
                    `)
            })
        }
        else {
            $(".day-overview-modal-holidays-events").html("אין מה להציג...")
        }

        $(".day-overview-modal-team-members").html(`${users.map(user => {
            let vacationDay = getVacationDayByUserId(today, user.userId)
            let opacity = vacationDay == null ? "100" : ["75", "75", "50"][vacationDay.status + 1]
            let statusText = vacationDay == null ? "נמצא/ה" : ["חופש לא אושר", "מחכה לאישור חופש", "בחופש"][vacationDay.status + 1]
            return `<span class="fw-bold opacity-${opacity}">${user.firstName} ${user.lastName}</span> - ${statusText}`
        }).join("<br>")}`)

        $(".day-overview-modal-title").html(`${dateFns.format(today, "dd.MM.yyyy")} - יום ${weekDayNames[today.getDay()]}`)
        $(".day-overview-modal").modal("show")
    })
})