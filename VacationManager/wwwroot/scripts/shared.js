import { HDate, gematriya, HebrewCalendar, Event, flags } from 'https://cdn.jsdelivr.net/npm/@hebcal/core@6.0.6/+esm';


var statsChart;
var statsMonth = new Date()
function drawChart(type, users, vacations) {
    let onlyMonthly = $(".stats-time-range").val() == "month"
    let labels = users.map(user => `${user.firstName} ${user.lastName}`)
    let data = [[], [], []]
    let colors = ["#ee8899", "#eecc77", "#88ee88"]
    let dayTypeWeights = [1, 0.5, 1, 1, 1]
    let chartType = "bar"
    users.forEach(user => {
        let userVacations = vacations.filter(vacation => vacation.vacation.userId == user.userId)
        let userWorkDayHours = user.workDayHours
        let cholHamoedMultiplier = (userWorkDayHours - 3) / userWorkDayHours

        let userData = [0, 0, 0]
        userVacations.forEach(vacation => {
            vacation.vacationDays.forEach(vacationDay => {
                var vacationDayWeight = dayTypeWeights[vacationDay.dayType - 1] * (isCholHaMoed(vacationDay.date) ? cholHamoedMultiplier : 1)
                console.log(vacationDay.date, isCholHaMoed(vacationDay.date))
                console.log(vacationDayWeight)
                switch (Number(type)) {
                    case 0:
                        if (vacationDay.date.getMonth() == (statsMonth).getMonth()) {
                            userData[vacationDay.status + 1] += vacationDayWeight
                        }
                        break
                    case 1:
                        if (vacationDay.date.getFullYear() == (new Date()).getFullYear()) {
                            userData[vacationDay.status + 1] += vacationDayWeight
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
function addStatsMonth(months) {
    statsMonth = dateFns.addMonths(statsMonth, months)
}
function subStatsMonth(months) {
    statsMonth = dateFns.subMonths(statsMonth, months)
}

function isCholHaMoed(date) {
    const events = HebrewCalendar.getHolidaysOnDate(date, true);
    return events?.some(event => event.getFlags() & flags.CHOL_HAMOED) ?? false;
}

export {
    statsChart, statsMonth,
    addStatsMonth, subStatsMonth,
    drawChart
}