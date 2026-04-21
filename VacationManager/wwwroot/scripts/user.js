import { HDate, gematriya, HebrewCalendar, Event } from 'https://cdn.jsdelivr.net/npm/@hebcal/core@6.0.6/+esm';
import {
    ping, repeat, getSelf, url,
    resetVacationDayInfo, toDate, getIsraelBusinessDays,
    getEvents, isEventDay, getTeams
} from "./utilities.js"

var self;

async function requestNewVacation(vacationRequest) {
    const request = new Request(`${url}/Vacations/Request`, {
        method: "POST",
        headers:
        {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${self.token}`
        },
        body: JSON.stringify(vacationRequest)
    })
    console.log(JSON.stringify(vacationRequest))
    let response = await fetch(request)
    let status = response.status
    if (response.ok) {
        $(".request-vacation-modal").modal("hide");
        $(".vacation-start").val("")
        $(".vacation-end").val("")
        $(".vacation-type").val("")
        resetVacationDayInfo()
        return true
    }
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
    console.log(vacationRequest)
    let response = await fetch(request)
    if (response.ok) {
        $(".request-vacation-modal").modal("hide");
        $(".vacation-start").val("")
        $(".vacation-end").val("")
        $(".vacation-type").val("")
        resetVacationDayInfo()
        return true
    }
    else {
        console.log(await response.text())
        return false
    }
}

async function getVacations() {
    console.log(self)
    const request = new Request(`${url}/Vacations/User/${self.user.userId}`, {
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
            })
            let cellEvents = isEventDay(added, events)
            cellEvents.forEach(cellEvent => {
                cellHolidays.push(`<span class="text-danger ${cellEvent.teamId == self.user.teamId ? "fw-bold" : ""}">${cellEvent.name}</span>`)
            })
            cellHtml += `<div class="fw-normal heb-date text-primary">${cellHolidays.join(`<br>`)}</div>`

            let vacationDay = getVacationDay(added)
            cellHtml += `<div class="mt-auto">`


            if (vacationDay != null) {
                let type = vacationDay.dayType
                let typeName = vacationTypes[type - 1]
                let status = vacationDay.status
                let statusEmoji = ['❗', '⏳', ''][status + 1]
                cellHtml += `<div class="day-type-${type} p-0 px-1 m-0 fw-normal rounded" style="font-size:85%">${statusEmoji} ${typeName}</div>`
            }
            else {
                cellHtml += `<div class="day-type-1 p-0 px-1 m-0 fw-normal rounded opacity-0">a</div>`
            }

            if (added.getMonth() != month) {
                cellClasses.push("text-black")
                cellClasses.push("text-opacity-25")
            }
            if (added.getDay() >= 5) {
                cellClasses.push("text-primary-emphasis bg-secondary bg-opacity-25")
                if (added.getMonth() != month) cellClasses.push("opacity-50")
            }

            cellHtml += `</div></div>`


            rowHtml += `<td class="${cellClasses.join(" ")}">${cellHtml}</td>`
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

function getVacationDay(date) {
    for (const vacation of vacations) {
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
            `<div class="vacation-day my-1 p-1 d-flex rounded fw-bold" data-day-type="${vacationTypes.indexOf(types[index]) + 1}">
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
        $(".vacation-days").children().eq(index).find("select").val(types[index])
    })
}
function buildVacationList() {
    $(".vacation-list").html("")
    vacations.forEach((vacation, index) => {
        let startDate = vacation.vacation.startDate
        let endDate = vacation.vacation.endDate
        let statusEmoji = ['&nbsp;❗', '⏳', '✅'][vacation.vacation.status + 1]
        $(".vacation-list").append(
            `
            <details ${openVacations[index] ? 'open' : ''}>
                <summary class="my-1">
                    ${dateFns.format(endDate, "dd/MM/yy")} - ${dateFns.format(startDate, "dd/MM/yy")} <span class="fs-5">${statusEmoji}</span>
                </summary>
                
                ${vacation.vacationDays.map(vacationDay => {
                let date = vacationDay.date
                let type = vacationDay.dayType
                let dayStatusEmoji = ['❌', '', '✔️'][vacationDay.status + 1]
                return `<div class="row">
                    <div class="col-9">
                    <div class="vacation-list-day day-type-${type} rounded pe-1 my-1">
                    ${dateFns.format(date, "dd/MM/yy")} - יום ${weekDays[date.getDay()]} - ${vacationTypes[type - 1]}
                    
                    </div>
                    </div>
                    <div class="col-3 fw-bold text-end p-0 my-1">${dayStatusEmoji}</div>
                    </div>`
            }).join("")}
                <button class="btn bg-secondary bg-opacity-50 my-1 delete-vacation-request">🗑</button>
                <button class="btn bg-secondary bg-opacity-50 my-1 edit-vacation-request">✏️</button>
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
            <details>
                <summary class="my-1">
                    <span class="text-decoration-underline">${name}: ${dateFns.format(endDate, "dd/MM/yy")} - ${dateFns.format(startDate, "dd/MM/yy")}</span>
                    </summary>
                ${otherTeam}
                ${description}
            </details>
        `)
    })
}

function vacationConflict() {
    $(".vacation-days").html(`<div class="border p-2 rounded vacation-selection-info">
                            כבר יש לך ימי חופשה בטווח הזה.
                        </div>`)
    $(".submit-request").addClass("disabled")
}

var monthNames = ['ינואר', 'פברואר', 'מרץ', 'אפריל', 'מאי', 'יוני', 'יולי', 'אוגוסט', 'ספטמבר', 'אוקטובר', 'נובמבר', 'דצמבר']
var weekDays = ['א', 'ב', 'ג', 'ד', 'ה', 'ו', 'ז']
var vacationTypes = ['יום חופש', 'חצי יום חופש', 'יום בחירה', 'יום הצהרה', 'היעדרות צפויה']
var currentDate = new Date()

var currentVacationDays = []
var startDate;
var endDate;

var teams;
var vacations;
var events;

var vacationToDeleteId;
var vacationToEdit;
var addOrEdit = "add"

var openVacations;

$(document).ready(async function () {

    $(".vacation-type").val("")

    self = getSelf()
    let check = await ping(self.token)
    if (!check) window.location.href = "./index.html"

    teams = await getTeams(self.token)
    vacations = await getVacations()
    events = await getEvents(self.token)
    console.log(events)

    openVacations = repeat(false, vacations.length)

    buildVacationList()
    buildEventList()

    renderCalendar(currentDate)
    updateCalendarNavigationLabel(currentDate)

    $(".account-header-name").html(`${self.user.email}`)

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
        $(".submit-request").html("שלח בקשה")
        $(".vacation-start").val("")
        $(".vacation-end").val("")
        $(".vacation-type").val("")
        resetVacationDayInfo()
        $(".request-vacation-modal").modal("show")
    })
    $(".vacation-parameter").change(function () {

        let start = $(".vacation-start").val()
        let end = $(".vacation-end").val()
        let type = $(".vacation-type").val()

        if (start == "" || end == "" || type == null) {
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

        $(".submit-request").removeClass("disabled")
        buildVacationDayEditor(vacationDays, repeat(type, vacationDays.length))
    })
    $(document).on("change", ".vacation-day select", function () {
        let type = $(this).val()
        let typeIndex = vacationTypes.indexOf(type) + 1
        $(this).parent().attr('data-day-type', typeIndex)
    })

    $(".submit-request").click(async function () {
        let dayTypes = []
        $.each($(".vacation-days").children(), function (index, day) {
            let type = Number($(day).attr("data-day-type"))
            dayTypes.push(type)
        })

        for (const vacationDay of currentVacationDays) {
            let vacationDayInfo = getVacationDay(vacationDay)
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
                if (await requestNewVacation(vacationRequest)) {
                    vacations = await getVacations()
                    openVacations.push(false)
                    buildVacationList()
                    renderCalendar(currentDate)
                }
                break;
            case "edit":
                if (await editVacation(vacationToEdit.vacation.vacationId, vacationRequest)) {
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
        let index = $(this).closest('details').index()
        vacationToDeleteId = vacations[index].vacation.vacationId

        let startDate = dateFns.format(vacations[index].vacation.startDate, "dd/MM/yy")
        let endDate = dateFns.format(vacations[index].vacation.endDate, "dd/MM/yy")

        $(".delete-vacation-modal-range").html(`${endDate} - ${startDate} ?`)
        $(".delete-vacation-modal").modal("show")
    })

    $(".delete-vacation-modal-confirm").click(async function () {
        let result = await deleteVacation(vacationToDeleteId)
        if (result) {
            vacations = await getVacations()
            openVacations.pop()
            buildVacationList()
            renderCalendar(currentDate)
            $(".delete-vacation-modal").modal("hide")
        }
    })

    $(document).on("click", ".edit-vacation-request", async function () {
        let index = $(this).closest('details').index()
        vacationToEdit = vacations[index]
        addOrEdit = "edit"

        $(".submit-request").html("שלח בקשה מחדש")
        $(".vacation-start").val(dateFns.format(vacationToEdit.vacation.startDate, "yyyy-MM-dd"))
        $(".vacation-end").val(dateFns.format(vacationToEdit.vacation.endDate, "yyyy-MM-dd"))

        let vacationDays = vacationToEdit.vacationDays.map(day => day.date)
        let vacationDayTypes = vacationToEdit.vacationDays.map(day => vacationTypes[day.dayType - 1])
        $(".vacation-type").val(vacationDayTypes[0])

        buildVacationDayEditor(vacationDays, vacationDayTypes)

        startDate = vacationToEdit.vacation.startDate
        endDate = vacationToEdit.vacation.endDate
        currentVacationDays = vacationDays

        $(".submit-request").removeClass("disabled")
        $(".request-vacation-modal").modal("show")
    })

    $(".account-header-logout").click(function () {
        localStorage.removeItem("token")
        localStorage.removeItem("user")
        window.location.href = "./index.html"
    })

    $(document).on("click", "details", function () {
        setTimeout(() => {
            openVacations[$(this).index()] = $(this).prop('open')
        }, 0)
    })
})