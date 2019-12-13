//Code to display banner at the top of every page with a count down to maintenance start time
//The date and time is set with the dateAsString variable

function maintenanceBanner(timeElement, inTimerElement, dateAsString, hoursOffset, siteName) {
    //We assume the time is in eastern time zone and append EST or EDT 
    //depending on whether daylight savings is applied or not.  The date
    //created from the parsed string will be localized for the user's 
    //current time zone.
    var dateString = dateAsString + " " + getTZString(); // dateAsString in form of "12/30/2016 2:00:00 PM"
    var countdownDate = new Date(Date.parse(dateString));

    countdownDate = new moment(countdownDate).add(hoursOffset, 'hours').toDate();

    //A function found on the internet to determine standard time zone offset
    function stdTimezoneOffset (date) {
        var jan = new Date(date.getFullYear(), 0, 1);
        var jul = new Date(date.getFullYear(), 6, 1);
        return Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
    }

    //Use the standard time zone off set and the current time zone offset
    //to determine if we are in daylight savings time.
    function isDST (date) {
        return date.getTimezoneOffset() < stdTimezoneOffset(date);
    }

    //Get the string to append to the date string for either eastern
    //standard time or daylight savings time.
    function getTZString(date)
    {
        var tz = "EST";
        var now = new Date();
        if (isDST(now))
        {
            tz = "EDT";
        }

        return tz;
    }

    function getCount(date, timerElement) {
        dateNow = new Date(); //get current date/time
        amount = date.getTime() - dateNow.getTime(); //calc countdown time in ms
        delete dateNow;

        if (amount < 0) {
            timerElement.html("Maintenance in progress...");
        } else {
            hours = 0;
            mins = 0;
            secs = 0;
            out = "";

            amount = Math.floor(amount / 1000); //convert from ms to s

            hours = Math.floor(amount / 3600);
            amount = amount % 3600;

            mins = Math.floor(amount / 60);
            amount = amount % 60

            secs = Math.floor(amount);

            if (hours != 0) {
                out += hours + " " + ((hours == 1) ? "hour" : "hours") + ", ";
            }
            if (mins != 0) {
                out += mins + " " + ((mins == 1) ? "minute" : "minutes") + ", ";
            }
            out += secs + " " + ((secs == 1) ? "second" : "seconds");
            timerElement.html(out);

            setTimeout(function () { getCount(date, timerElement) }, 1000);
        }
    }

    $(document).ready(function () {
        var beginning = "The site";

        if (siteName && siteName !== '') {
            beginning = "The " + siteName + " site(s)";
        } 

        timeElement.html(beginning + " will be shutting down for maintenance at " + countdownDate.toLocaleTimeString() + " on " + countdownDate.toLocaleDateString() + ". ");

        getCount(countdownDate, inTimerElement);
    });
}
