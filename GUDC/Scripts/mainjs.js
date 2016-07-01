var flag = false;
var map = null;
var marker = null;
var tasksMarkers = [];
var teamMarkers = {};
var directionsDisplay = null;
//var chartGoogleMarker = null;
var csvData = null;
var polyRoute = null;
var addTaskTeamMaker = null;
var googleChartArray = [];
var selectedTaskMarker = null;
var acceptTaskMarker = null;
var mobileTaskCounters = 0;
var geocoder = null;
var extentFlag = true;
//var tracktaskList = [];
var trackRouteList = [];

var timeInterval;
var x = 31.0;
var y = 31.0;
var routeIndex = 0;

google.maps.Marker.prototype.setLabel = function (label) {
    this.label = new MarkerLabel({
        map: this.map,
        marker: this,
        text: label
    });
    this.label.bindTo('position', this, 'position');
};

var MarkerLabel = function (options) {
    this.setValues(options);
    this.span = document.createElement('span');
    this.span.className = 'map-marker-label';
};

MarkerLabel.prototype = $.extend(new google.maps.OverlayView(), {
    onAdd: function () {
        this.getPanes().overlayImage.appendChild(this.span);
        var self = this;
        this.listeners = [
          google.maps.event.addListener(this, 'position_changed', function () {
              self.draw();
          })
        ];
    },
    draw: function () {
        var text = String(this.get('text'));
        var position = this.getProjection().fromLatLngToDivPixel(this.get('position'));
        this.span.innerHTML = text;
        this.span.style.left = (position.x - (markerSize.x / 2)) - (text.length * 3) + 10 + 'px';
        this.span.style.top = (position.y - markerSize.y + 40) + 'px';
    }
});

var acceptTaskFlag = false;
function initialize() {

    if (document.getElementById("googleMap") != null) {
        geocoder = new google.maps.Geocoder();
        directionsDisplay = new google.maps.DirectionsRenderer();// also, constructor can get "DirectionsRendererOptions" object
        directionsDisplay.setMap(map);
        map = new google.maps.Map(document.getElementById('googleMap'), {
            center: new google.maps.LatLng(30.0333, 31.2167),
            zoom: 6,
            disableDefaultUI: true,
            zoomControl: true,
            zoomControlOptions: {
                style: google.maps.ZoomControlStyle.LARGE,
                position: google.maps.ControlPosition.LEFT_TOP
            },
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });

        //var infoWindow = new google.maps.Map(document.getElementById("googleMap"), map);
        //var infoWindow = new google.maps.InfoWindow({map: map});


        if (document.getElementById("taskCoordinate") != null && $("#taskCoordinate").val() != "") {
            mapSetCenter($("#taskCoordinate").val());
            map.setZoom(13);
            marker = placeMarker($("#taskCoordinate").val(), $("#taskStatus").val());
        }
        /////////////////////////////////mobile task display///////////////////
        if (document.getElementById("MobileTeamLocation") != null) {
            loadMobileTask();
            function loadMobileTask() {
                var param = { teamName: $('#MobileTeamName').val() }
                $.ajax({
                    type: "POST",
                    url: "Mobile/readTask",
                    data: param,
                    dataType: "json",
                    success: function (data) { processTaskData(data); }
                });
                function processTaskData(data) {

                    if (data != null && data.length > 0) {
                        if (mobileTaskCounters == 0) {
                            tasksMarkers = [];
                            mobileTaskCounters = data.length;
                            drawTaskMarkers(data);
                        }
                        else if (mobileTaskCounters < data.length) {
                            if (tasksMarkers.length > 0) {
                                for (var k = 0; k < tasksMarkers.length; k++) {
                                    tasksMarkers[k].setMap(null);
                                }
                            }
                            tasksMarkers = [];
                            mobileTaskCounters = data.length;
                            drawTaskMarkers(data);
                            app.newTaskAssigned(data[data.length - 1].TASK);
                        }
                    }
                    setTimeout(function () { loadMobileTask() }, 10000);
                }

            }
            //tasksMarkers = [];
            //var rows = document.getElementById("taskTbl").rows;
            //for (var i = 1, ceiling = rows.length; i < ceiling; i++) {
            //    var loc = rows[i].cells[5].innerHTML;
            //    //var status = rows[i].cells[1].innerHTML;
            //    if (loc != "") {
            //        var marker = placeMarker(loc, "Open");

            //        if (marker != null) {
            //            marker.setTitle(rows[i].cells[0].innerHTML);
            //            marker.info = new google.maps.InfoWindow({
            //                content: '<b>Task Name:</b> ' + rows[i].cells[0].innerHTML + '<br><b>Start:</b>' + 'rows[i].cells[2].innerHTML'
            //                    + '<br><b>Detail:</b>' + 'rows[i].cells[6].innerHTML'
            //            });
            //            google.maps.event.addListener(marker, 'click', function () {
            //                //var selectedTaskLoc = marker.getPosition();
            //                document.getElementById("MobileTaskLocation").value = marker.getPosition().lat() + "," + marker.getPosition().lng();
            //                selectedTaskMarker = marker;
            //                marker.info.open(map, marker);
            //            });
            //            tasksMarkers.push(marker);
            //        }
            //    }
        }
        //if ($("#MobileTaskLocation").val() != "") {
        //    tasksMarkers.push(placeMarker($("#MobileTaskLocation").val(), "Open"));
        //}
        //if ($("#MobileTeamLocation").val() != "") {
        //    tasksMarkers.push(mobileTeamMarker($("#MobileTeamLocation").val(), $("#MobileTeamName").val()));
        //}
        ////displayTeamChange();
        //setFitBound(map);
        /////////////////text search///////////////
        if (document.getElementById("txtPlaces") != null) {
            var places = new google.maps.places.Autocomplete(document.getElementById('txtPlaces'));
            var from = null;
            google.maps.event.addListener(places, 'place_changed', function () {
                from = places.getPlace();
            });

            document.getElementById("txtPlaces").addEventListener("keypress", function (e) {
                if (e.keyCode == 13) {
                    if (from == null) {
                        alert("You have to select an origin");
                    } else {
                        map.setCenter(new google.maps.LatLng(from.geometry.location.lat(), from.geometry.location.lng()));
                        map.setZoom(13);
                    }
                }
            });
        }
        if (document.getElementById("searchsubmit") != null) {
            document.getElementById("searchsubmit").addEventListener("click", function () {
                if (from == null) {
                    alert("You have to select an origin");
                } else {
                    map.setCenter(new google.maps.LatLng(from.geometry.location.lat(), from.geometry.location.lng()));
                    map.setZoom(13);
                }
            });
        }
        ///////////////////////////////////////////////////////////
        if (document.getElementById("tasksTable") != null) {
            initTasks();
        }
        if (document.getElementById("districtReTbl") != null) {
            drawDistrictTable();
        }
        google.maps.event.addListener(map, 'click', function (event) {
            if (document.getElementById("taskLocation") != null) {
                if (marker != null) {
                    marker.setMap(null)
                }
                var locationData = event.latLng.lat() + "," + event.latLng.lng();
                $("#taskCoordinate").val(locationData);
                marker = placeMarker(locationData, $("#taskStatus").val());
                //addMarker(event.latLng);
                getAddress(event.latLng);
            }

        });

        function addMarker(location) {
            if (marker != null) {
                marker.setMap(null)
            }
            marker = new google.maps.Marker({
                position: location,
                map: map,
                //draggable: true,
                icon: "http://maps.google.com/mapfiles/ms/icons/red-dot.png"
            });

        }
    }
}
function getAddress(latLng) {
    geocoder.geocode({ 'latLng': latLng },
      function (results, status) {
          if (status == google.maps.GeocoderStatus.OK) {
              if (results[0]) {
                  document.getElementById("taskLocation").value = results[0].formatted_address;
              }
              else {
                  document.getElementById("taskLocation").value = "No results";
              }
          }
          else {
              document.getElementById("taskLocation").value = status;
          }
      });
}

function placeMarker(location, status) {
    var latlon = location.split(",");
    if (latlon[0] == null || latlon[0] == "") return null;
    if (latlon[1] == null || latlon[1] == "") return null;

    //var image = 'Images/search.png';
    var myLatlng = new google.maps.LatLng(latlon[0], latlon[1]);

    if (status == "Open") {
        var markerTask = new google.maps.Marker({
            position: myLatlng,
            map: map,
            clickable: true,
            icon: "http://maps.google.com/mapfiles/ms/icons/red-dot.png"
        });

    }
    else if (status == "Pending") {
        var markerTask = new google.maps.Marker({
            position: myLatlng,
            map: map,
            //draggable: true,
            icon: "http://maps.google.com/mapfiles/ms/icons/yellow-dot.png"
        });
    }
    else if (status == "Closed") {
        var markerTask = new google.maps.Marker({
            position: myLatlng,
            map: map,
            //draggable: true,
            icon: "http://maps.google.com/mapfiles/ms/icons/green-dot.png"
        });
    }
    else {
        var markerTask = new google.maps.Marker({
            position: myLatlng,
            map: map,
            //draggable: true,
            icon: "http://maps.google.com/mapfiles/ms/icons/blue-dot.png"
        });
    }

    // To add the marker to the map, call setMap();
    markerTask.setMap(map);
    return markerTask;
}
function setFlagAddTask() {
    document.body.style.cursor = "crosshair";
    flag = true;
}
function mapSetCenter(location) {
    var latlon = location.split(",");

    var myLatlng = new google.maps.LatLng(latlon[0], latlon[1]);
    map.setCenter(myLatlng);
}
function backFunction() {
    window.history.back()
}
function forwardFunction() {
    window.history.forward()
}
function mainPageLoad() {
    var height = document.getElementById("heigtBody");
    if (document.getElementById("heigtBody").clientHeight < (document.documentElement.clientHeight - 142)) {
        document.getElementById("heigtBody").style.height = (document.documentElement.clientHeight - 142) + 'px';
    }
    else {
        document.getElementById("heigtBody").style.height = "auto";
    }
}
function goTeam(tdTeam) {
    var value = tdTeam.innerHTML;
    window.location.assign("Teams/AddEditTeam/");
}
function initTasks() {
    setMapOnAll(null);
    var rows = document.getElementById("tasksTable").rows;

    ////////////////////////////////////////////////////////////////////////
    for (var i = 1, ceiling = rows.length; i < ceiling; i++) {
        var loc = rows[i].cells[7].innerHTML;
        var status = rows[i].cells[1].innerHTML;
        var teamName = rows[i].cells[0].innerHTML;

        if (loc != "") {
            if (status == "free" || status == "on mission" || status == "out of service") {
                var marker = mobileTeamMarker(loc, rows[i].cells[0].innerHTML);
                if (marker != null) {
                    tasksMarkers.push(marker);
                }
            }
            else {
                var marker = placeMarker(loc, status);
                if (marker != null) {
                    if (status == "Open") {
                        marker.setTitle("This task is attached to the " + teamName);

                        google.maps.event.addListener(marker, 'click', (function (marker) {
                            return function () {
                                selectedTaskMarker = marker;
                            };
                        })(marker));
                    }
                    tasksMarkers.push(marker);
                }
            }
        }
    }
    ///////////////////////////////////////////////////////
    if (document.getElementById("dashTaskTable") != null) {
        loadTeam();
    }
    else {
        setFitBound(map);
    }

}
function loadTeam() {
    $.ajax({
        type: "POST",
        url: "DashBoard/readTeamLocation",
        dataType: "json",
        success: function (data) { processTeamData(data); }
    });
    function processTeamData(data) {
        if (data != null) {
            for (var i = 0; i < teamMarkers.length; i++) {
                teamMarkers[i].setMap(null);
            }
            //teamMarkers = [];

            for (var j = 0, ceiling = data.length; j < ceiling; j++) {
                if (data[j].TEAMLOCATION != null && data[j].TEAMLOCATION != "") {
                    if (teamMarkers[data[j].TEAMNAME] != null) {
                        var latlon = data[j].TEAMLOCATION.split(",");

                        var myLatlng = new google.maps.LatLng(latlon[0], latlon[1]);
                        teamMarkers[data[j].TEAMNAME].setPosition(myLatlng)
                    }
                    else {
                        var marker = mobileTeamMarker(data[j].TEAMLOCATION, data[j].TEAMNAME);
                        if (marker != null) {
                            teamMarkers[data[j].TEAMNAME] = marker;
                        }
                    }
                }
            }
            if (extentFlag) {
                setFitBound(map);
                extentFlag = false;
            }
        }
        setTimeout(function () { loadTeam() }, 10000);
    }
}

function setFitBound(mapCanvas) {
    //if (tasksMarkers.length == 0) {
    //    return;
    //}
    var bounds = new google.maps.LatLngBounds();
    for (i = 0; i < tasksMarkers.length; i++) {
        bounds.extend(tasksMarkers[i].getPosition());
    }
    for (x in teamMarkers) {
        bounds.extend(teamMarkers[x].getPosition());
    }

    if (tasksMarkers.length == 1 && teamMarkers.length < 1) {
        google.maps.event.addListenerOnce(mapCanvas, 'bounds_changed', function (event) {
            mapCanvas.setZoom(mapCanvas.getZoom() - 1);

            if (this.getZoom() > 15) {
                this.setZoom(15);
            }
        });
    }

    mapCanvas.setCenter(bounds.getCenter());
    mapCanvas.fitBounds(bounds);
}
function setMapOnAll(map) {
    for (var i = 0; i < tasksMarkers.length; i++) {
        tasksMarkers[i].setMap(map);
    }
    tasksMarkers = [];
}
function activeTaskDataView(trData) {
    var tdCoor = trData.cells[7];
    var locationData = tdCoor.innerHTML;

    mapSetCenter(locationData);
    //alert(taskDataList);
}
function displayTeam(selectTeam) {
    //if ($("#taskCoordinate").val() == '') {
    //    alert("Please click task location on map!");
    //    document.getElementById("taskTeamname").selectedIndex = -1;
    //    return;
    //}
    var param = { teamname: selectTeam.value }
    $.ajax({
        type: "POST",
        url: "readTeamLocation",
        data: param,
        dataType: "json",
        success: function (data) { processData(data); }
    });
    function processData(allText) {

        if (allText != null && allText != "") {
            if (marker != null) {
                marker.setMap(null)
            }
            //var locationData = event.latLng.lat() + "," + event.latLng.lng();
            $("#taskCoordinate").val(allText);
            marker = placeMarker(allText, $("#taskStatus").val());
            var latlon = allText.split(",");
            if (latlon[0] == null || latlon[0] == "") return null;
            if (latlon[1] == null || latlon[1] == "") return null;
            var myLatlng = new google.maps.LatLng(latlon[0], latlon[1]);
            getAddress(myLatlng);
        }
    }
}
function dashboardDrawRoute() {
    if (selectedTaskMarker == null) {
        alert("You have to select an Openning Task.");
        return;
    }
    var titleName = selectedTaskMarker.title;
    var teamName = titleName.replace("This task is attached to the ", "");
    if (teamMarkers[teamName] != null) {
        var start = selectedTaskMarker.getPosition().lat() + "," + selectedTaskMarker.getPosition().lng();
        var end = teamMarkers[teamName].getPosition().lat() + "," + teamMarkers[teamName].getPosition().lng();
        displayRoute(start, end, false);
    }
    else {
        alert("Can not find " + teamName + " Team Location.");
        return;
    }
}

function displayRoute(start, end, drawFirstMakerFlag) {
    if (polyRoute != null) polyRoute.setMap(null);
    var startList = start.split(",");
    if (startList[0] == null || startList[0] == "") return;
    if (startList[1] == null || startList[1] == "") return;
    var startPoint = new google.maps.LatLng(startList[0], startList[1]);

    var endList = end.split(",");
    if (endList[0] == null || endList[0] == "") return;
    if (endList[1] == null || endList[1] == "") return;
    var endPoint = new google.maps.LatLng(endList[0], endList[1]);

    //Intialize the Path Array
    var path = new google.maps.MVCArray();

    //Intialize the Direction Service
    var service = new google.maps.DirectionsService();

    //Set the Path Stroke Color

    polyRoute = new google.maps.Polyline({ map: map, strokeColor: '#4986E7' });

    path.push(startPoint);
    polyRoute.setPath(path);
    service.route({
        origin: startPoint,
        destination: endPoint,
        travelMode: google.maps.DirectionsTravelMode.DRIVING
    }, function (result, status) {
        if (status == google.maps.DirectionsStatus.OK) {

            for (var i = 0, len = result.routes[0].overview_path.length; i < len; i++) {
                path.push(result.routes[0].overview_path[i]);
            }
            //var strRoute = JSON.stringify(result.routes[0].overview_path);
            //$("#route").val(strRoute);

        }
    });
    if (drawFirstMakerFlag) {
        if (addTaskTeamMaker != null) {
            addTaskTeamMaker.setMap(null)
        }
        addTaskTeamMaker = new google.maps.Marker({
            position: startPoint,
            map: map,
            //draggable: true,
            icon: "http://maps.google.com/mapfiles/ms/icons/blue-dot.png"
        });

    }

}


function drawDistrictTable(mapdiv) {
    var param = { district: $('#DistrictLists').val(), start: $('#Start').val(), end: $('#End').val() }
    $.ajax({
        type: "POST",
        url: "setDistrictTable",
        data: param,
        dataType: "text",
        success: function (data) {
            var tasks = JSON.parse(data);
            $('#taskBodyTable').empty();
            var chartData = [['TaskStatus', 'Count'], ['Open', 0], ['Pending', 0], ['Closed', 0]];
            var flag = false;
            for (var i = 0; i < tasks.length ; i++) {
                if (tasks[i].STATUS == "Open") {
                    chartData[1][1] = chartData[1][1] + 1;
                }
                else if (tasks[i].STATUS == "Pending") {
                    chartData[2][1] = chartData[2][1] + 1;
                }
                else if (tasks[i].STATUS == "Closed") {
                    chartData[3][1] = chartData[3][1] + 1;
                }

                if (flag == false) {
                    var text = "<tr class =\"odd\"><td>" + tasks[i].TASK + "</td><td>" + tasks[i].STATUS + "</td><td>" + getDateTimeCode(tasks[i].START) + "</td><td>" + getDateTimeCode(tasks[i].END) + "</td><td>" + tasks[i].LOCATION + "</td><td>" + tasks[i].DISTRICT + "</td></tr>"
                    $('#taskBodyTable').append(text);
                }
                else {
                    var text = "<tr class =\"even\"><td>" + tasks[i].TASK + "</td><td>" + tasks[i].STATUS + "</td><td>" + getDateTimeCode(tasks[i].START) + "</td><td>" + getDateTimeCode(tasks[i].END) + "</td><td>" + tasks[i].LOCATION + "</td><td>" + tasks[i].DISTRICT + "</td></tr>"
                    $('#taskBodyTable').append(text);
                }
                flag = !flag;
            }
            if (tasks.length > 0) drawGoogleChart(chartData, $('#DistrictLists').val(), mapdiv);
            mainPageLoad();
        }
    });
}
function drawTable() {
    var param = { teamname: $('#teamName').val(), status: $('#taskStatus').val(), district: $('#DistrictLists').val(), start: $('#Start').val(), end: $('#End').val() }
    $.ajax({
        type: "POST",
        url: "setTable",
        data: param,
        dataType: "text",
        success: function (data) { reportProcessData(data); }
    });
}
function reportProcessData(data) {
    var tasks = JSON.parse(data);
    $('#taskBodyTable').empty();
    $('#chartdiv').empty();
    var flag = false;
    var chartData = [];

    for (var i = 0; i < tasks.length ; i++) {
        if (i == 0) {
            var chartDataObject = { 'TEAMNAME': tasks[i].TEAMNAME, 'OPEN': 0, 'PENDING': 0, 'CLOSED': 0 };
            chartData.push(setValueChartData(chartDataObject, tasks[i].STATUS));
        }
        else {
            var findflag = false;
            for (var k = 0; k < chartData.length ; k++) {
                if (chartData[k].TEAMNAME == tasks[i].TEAMNAME) {
                    setValueChartData(chartData[k], tasks[i].STATUS);
                    findflag = true;
                    break;
                }
            }
            if (!findflag) {
                //chartDataObject = { 'TEAMNAME': tasks[i].TEAMNAME, 'OPEN': 0, 'PENDING': 0, 'CLOSED': 0 };
                chartData.push(setValueChartData({ 'TEAMNAME': tasks[i].TEAMNAME, 'OPEN': 0, 'PENDING': 0, 'CLOSED': 0 }, tasks[i].STATUS));
            }

        }

        if (flag == false) {
            var text = "<tr class =\"odd\"><td>" + tasks[i].TASK + "</td><td>" + tasks[i].STATUS + "</td><td>" + getDateTimeCode(tasks[i].START) + "</td><td>" + getDateTimeCode(tasks[i].END) + "</td><td>" + tasks[i].LOCATION + "</td><td>" + tasks[i].DISTRICT + "</td></tr>"
            $('#taskBodyTable').append(text);
            //document.getElementById("heigtBody").style.height = (document.getElementById("heigtBody").clientHeight + 10) + 'px';

        }
        else {
            var text = "<tr class =\"even\"><td>" + tasks[i].TASK + "</td><td>" + tasks[i].STATUS + "</td><td>" + getDateTimeCode(tasks[i].START) + "</td><td>" + getDateTimeCode(tasks[i].END) + "</td><td>" + tasks[i].LOCATION + "</td><td>" + tasks[i].DISTRICT + "</td></tr>"
            $('#taskBodyTable').append(text);
            //document.getElementById("heigtBody").style.height = (document.getElementById("heigtBody").clientHeight + 10) + 'px';

        }
        flag = !flag;
    }
    drawChart(chartData);

    mainPageLoad();
}

function getDateTimeCode(str) {
    var value = str.substring(6, str.length - 2);
    var datetime = new Date(parseInt(value));
    return datetime.toLocaleDateString() + " " + datetime.toLocaleTimeString();
}

function setValueChartData(chartDataObject, status) {
    var chartItem = chartDataObject
    if (status == "Open") {
        chartItem.OPEN = chartDataObject.OPEN + 1;
    }
    else if (status == "Pending") {
        chartItem.PENDING = chartDataObject.PENDING + 1;
    }
    else if (status == "Closed") {
        chartItem.CLOSED = chartDataObject.CLOSED + 1;
    }
    return chartItem;
}

function drawChart(chartData) {
    chart = new AmCharts.AmSerialChart();
    chart.dataProvider = chartData;
    chart.categoryField = "TEAMNAME";
    chart.startDuration = 1;
    chart.plotAreaBorderColor = "#DADADA";
    chart.plotAreaBorderAlpha = 1;
    // this single line makes the chart a bar chart
    //chart.rotate = false;
    chart.depth3D = 10;
    chart.angle = 20;


    // AXES
    // Category
    var categoryAxis = chart.categoryAxis;
    categoryAxis.gridPosition = "start";
    categoryAxis.gridAlpha = 0.1;
    categoryAxis.axisAlpha = 0;

    // Value
    var valueAxis = new AmCharts.ValueAxis();
    valueAxis.axisAlpha = 0;
    valueAxis.gridAlpha = 0.1;
    valueAxis.position = "left";
    chart.addValueAxis(valueAxis);

    // GRAPHS
    // first graph
    var graph1 = new AmCharts.AmGraph();
    graph1.type = "column";
    graph1.title = "OPEN";
    graph1.valueField = "OPEN";
    graph1.balloonText = "OPEN:[[value]]";
    graph1.lineAlpha = 0;
    graph1.fillColors = "#65659A";
    graph1.fillAlphas = 1;
    chart.addGraph(graph1);

    // second graph
    var graph2 = new AmCharts.AmGraph();
    graph2.type = "column";
    graph2.title = "PENDING";
    graph2.valueField = "PENDING";
    graph2.balloonText = "PENDING:[[value]]";
    graph2.lineAlpha = 0;
    graph2.fillColors = "#FD9B00";
    graph2.fillAlphas = 1;
    chart.addGraph(graph2);

    var graph3 = new AmCharts.AmGraph();
    graph3.type = "column";
    graph3.title = "CLOSED";
    graph3.valueField = "CLOSED";
    graph3.balloonText = "CLOSED:[[value]]";
    graph3.lineAlpha = 0;
    graph3.fillColors = "#B4CD9B";
    graph3.fillAlphas = 1;
    chart.addGraph(graph3);


    // LEGEND
    var legend = new AmCharts.AmLegend();
    chart.addLegend(legend);

    chart.creditsPosition = "top-right";

    // WRITE
    chart.write("chartdiv");

}

function drawGoogleChart(chartData, district, mapdiv) {
    if (googleChartArray.length > 0) {
        for (var i = 0, ceiling = googleChartArray.length; i < ceiling; i++) {
            googleChartArray[i].onRemove();
        }
        googleChartArray = [];
    }
    var latLng;
    tasksMarkers = [];
    if (district == "All") {
        var districtArray = [];
        var statusArray = [];
        var chartData = [['TaskStatus', 'Count'], ['Open', 0], ['Pending', 0], ['Closed', 0]];
        var rows = document.getElementById("taskBodyTable").rows;
        for (var i = 0, ceiling = rows.length; i < ceiling; i++) {
            var districs = rows[i].cells[5].innerHTML;
            var status = rows[i].cells[1].innerHTML;
            var districItems = districs.split(',');
            for (var j = 0, count = districItems.length; j < count; j++) {
                if (districtArray.length > 0 && districtArray.indexOf(districItems[j]) >= 0) {
                    var index = districtArray.indexOf(districItems[j]);
                    if (status == "Open") {
                        statusArray[index][1][1] = statusArray[index][1][1] + 1;
                    }
                    else if (status == "Pending") {
                        statusArray[index][2][1] = statusArray[index][2][1] + 1;
                    }
                    else if (status == "Closed") {
                        statusArray[index][3][1] = statusArray[index][3][1] + 1;
                    }
                }
                else {
                    var index = districtArray.indexOf(districItems[j]);
                    if (status == "Open") {
                        statusArray.push([['TaskStatus', 'Count'], ['Open', 1], ['Pending', 0], ['Closed', 0]]);
                    }
                    else if (status == "Pending") {
                        statusArray.push([['TaskStatus', 'Count'], ['Open', 0], ['Pending', 1], ['Closed', 0]]);
                    }
                    else if (status == "Closed") {
                        statusArray.push([['TaskStatus', 'Count'], ['Open', 0], ['Pending', 0], ['Closed', 1]]);
                    }
                    districtArray.push(districItems[j]);
                }
            }
        }
        for (var j = 0, count = districtArray.length; j < count; j++) {
            makeGoogleChart(districtArray[j], statusArray[j]);
        }

    }
    else {
        makeGoogleChart(district, chartData);
    }

    function makeGoogleChart(districtName, chartData1) {
        var data = new google.visualization.arrayToDataTable(chartData1);
        var options = { title: districtName, fontSize: 8 };
        districtName = districtName + "Egypt";
        var service;
        service = new google.maps.places.PlacesService(mapdiv);
        var request = {
            query: districtName
        };
        service.textSearch(request, callback);

        function callback(results, status) {
            if (status == google.maps.places.PlacesServiceStatus.OK && results.length > 0) {
                mapdiv.setCenter(results[0].geometry.location);
                latLng = results[0].geometry.location;
                var markerTask = new google.maps.Marker({
                    position: latLng,
                    map: mapdiv,
                });
                tasksMarkers.push(markerTask);
                var chartGoogleMarker = new ChartMarker({
                    map: mapdiv,
                    position: latLng,
                    width: '250px',
                    height: '100px',
                    chartData: data,
                    chartOptions: options,
                });
                googleChartArray.push(chartGoogleMarker)
                setFitBound(mapdiv);

            }
            //else {
            //    latLng = new google.maps.LatLng(30.0333, 31.2167);
            //}
        }

    }

}

////////////////////////////mobile functions///////////////////////////////

function displayMobile(mbClickStatus) {
    if (mbClickStatus == "TaskStatus") {
        document.getElementById("TaskStatus").style.backgroundColor = "#1d72ef";
        document.getElementById("StatusDiv").style.left = (document.documentElement.clientWidth - 220) / 2 + 'px';
        document.getElementById("StatusDiv").style.top = (document.documentElement.clientHeight - 163) / 2 + 'px';
        document.getElementById("StatusDiv").style.zIndex = "999";
        document.getElementById("StatusDiv").style.display = "block";
    }
    else {
        document.getElementById("TaskStatus").style.backgroundColor = "#222222";
        document.getElementById("StatusDiv").style.display = "none";

    }
    if (mbClickStatus == "BestRoute") {
        document.getElementById("BestRoute").style.backgroundColor = "#1d72ef";
    }
    else {
        document.getElementById("BestRoute").style.backgroundColor = "#222222";
    }
    if (mbClickStatus == "ArrivedTask") {
        document.getElementById("ArrivedTask").style.backgroundColor = "#1d72ef";
    }
    else {
        document.getElementById("ArrivedTask").style.backgroundColor = "#222222";
    }
    if (mbClickStatus == "AcceptTask") {
        document.getElementById("AcceptTask").style.backgroundColor = "#1d72ef";
        document.getElementById("AcceptDiv").style.left = (document.documentElement.clientWidth - 220) / 2 + 'px';
        document.getElementById("AcceptDiv").style.top = (document.documentElement.clientHeight - 163) / 2 + 'px';

        document.getElementById("AcceptDiv").style.zIndex = "999";
        document.getElementById("AcceptDiv").style.display = "block";

    }
    else {
        document.getElementById("AcceptTask").style.backgroundColor = "#222222";
        document.getElementById("AcceptDiv").style.display = "none";
    }
}
function TaskStatusClick() {
    displayMobile("TaskStatus");
}
function ArrivedTaskClick() {
    //displayMobile("ArrivedTask");
    routeIndex++;
    var param = { taskname: acceptTaskMarker.getTitle(), teamname: $("#MobileTeamName").val(), index: routeIndex }
    //clearInterval(timeInterval);
    $.ajax({
        type: "POST",
        url: "Mobile/TaskArrived",
        data: param,
        dataType: "text",
        success: function (data) { resultTask(data); }
    });
    acceptTaskFlag = false;
    routeIndex = 0;
}
function BestRouteClick() {
    //if (acceptTaskFlag)
    {
        displayMobile("BestRoute");
        if ($("#MobileTaskLocation").val() != null && $("#MobileTaskLocation").val() != "" && $("#MobileTeamLocation").val() != null && $("#MobileTeamLocation").val() != "") {
            displayRoute($("#MobileTeamLocation").val(), $("#MobileTaskLocation").val(), false);
        }

    }
}

function TaskAccept() {
    deleteTracks();
    trackRouteList = [];
    acceptTaskFlag = true;
    if (selectedTaskMarker != null) {
        acceptTaskMarker = selectedTaskMarker;
    }
    var param = { teamloc: $("#MobileTeamLocation").val(), teamname: $("#MobileTeamName").val(), taskname: acceptTaskMarker.getTitle() }
    //var param = { teamloc: $("#MobileTeamLocation").val(), teamname: $("#MobileTeamName").val(), taskname: "task1"}

    $.ajax({
        type: "POST",
        url: "Mobile/TaskAccept",
        data: param,
        dataType: "text",
        success: function (data) { resultTask(data); }
    });

    document.getElementById("AcceptDiv").style.display = "none";
    routeIndex = 0;

    //timeInterval = setInterval(testSaveLoction(), 1000)
}

function testSaveLoction() {
    x = x + 0.1;
    y = y + 0.1;
    routeIndex++;
    var param = { teamLocation: x.toString() + "," + y.toString(), username: "jin1", taskname: "task1", "isaccept": routeIndex }
    $.ajax({
        type: "POST",
        url: "Mobile/ChangeTeamLocation",
        data: param,
        dataType: "text",
        success: function (data) { return; }
    });
}


function TaskReject() {
    acceptTaskFlag = false;
    //document.getElementById("AcceptDiv").style.display = "none";

}
function TaskClosed() {
    var param = { status: "Closed", teamname: $("#MobileTeamName").val(), taskname: acceptTaskMarker.getTitle() }
    $.ajax({
        type: "POST",
        url: "Mobile/TaskStatus",
        data: param,
        dataType: "text",
        success: function (data) { resultTask(data); }
    });
    document.getElementById("StatusDiv").style.display = "none";

    if ($("#MobileTaskLocation").val() != "" && tasksMarkers.length > 1) {
        tasksMarkers[0].setIcon("http://maps.google.com/mapfiles/ms/icons/green-dot.png");
    }
}
function TaskPending() {
    var param = { status: "Pending", teamname: $("#MobileTeamName").val(), taskname: acceptTaskMarker.getTitle() }
    $.ajax({
        type: "POST",
        url: "Mobile/TaskStatus",
        data: param,
        dataType: "text",
        success: function (data) { resultTask(data); }
    });
    document.getElementById("StatusDiv").style.display = "none";
    if ($("#MobileTaskLocation").val() != "" && tasksMarkers.length > 1) {
        tasksMarkers[0].setIcon("http://maps.google.com/mapfiles/ms/icons/yellow-dot.png");
    }
}

function resultTask(data) {
    routeIndex = 0;
    return;
}
function changeTeam(location) {
    if (location != null && location != "") {

        if ($("#MobileTeamLocation").val() != "" && tasksMarkers.length > 0) {
            var latlon = location.split(",");
            if (latlon[0] == null || latlon[0] == "") return null;
            if (latlon[1] == null || latlon[1] == "") return null;
            var myLatlng1 = new google.maps.LatLng(latlon[0], latlon[1]);
            if (acceptTaskFlag) {

                var strFp = $("#MobileTeamLocation").val();
                var latlon1 = strFp.split(",");
                if (latlon1[0] == null || latlon1[0] == "") return null;
                if (latlon1[1] == null || latlon1[1] == "") return null;
                myLatlng2 = new google.maps.LatLng(latlon1[0], latlon1[1]);
                var ptArr = [];
                ptArr.push(myLatlng1);
                ptArr.push(myLatlng2);
                var polyline = makePolyLineMobile(ptArr);
                polyline.setMap(map);
                trackRouteList.push(polyline);
            }


            tasksMarkers[tasksMarkers.length - 1].setPosition(myLatlng1);
        }
        else {
            tasksMarkers.push(mobileTeamMarker(location, $("#MobileTeamName").val()));
        }
        document.getElementById("MobileTeamLocation").value = location;
        setFitBound(map);

        var param;
        if (acceptTaskFlag) {
            routeIndex++;
            param = { teamLocation: location, teamname: $("#MobileTeamName").val(), taskname: acceptTaskMarker.getTitle(), "isaccept": routeIndex }
        } else {
            param = { teamLocation: location, teamname: $("#MobileTeamName").val(), taskname: "", "isaccept": 0 }

        }
        $.ajax({
            type: "POST",
            url: "Mobile/ChangeTeamLocation",
            data: param,
            dataType: "text",
            success: function (data) { return; }
        });

    }
}
function mobileTeamMarker(location, txtLabel) {
    var latlon = location.split(",");
    if (latlon[1] == null || latlon[1] == "") return null;
    if (latlon[0] == null || latlon[0] == "") return null;
    var myLatlng = new google.maps.LatLng(latlon[0], latlon[1]);
    var markerTask = new google.maps.Marker({
        position: myLatlng,
        map: map,
        icon: "http://maps.google.com/mapfiles/ms/icons/blue-dot.png",
        label: txtLabel
    });

    // To add the marker to the map, call setMap();
    markerTask.setMap(map);
    return markerTask;
}
var markerSize = {
    x: 22,
    y: 40
};

function IsArrived(teamLoc) {
    //app.isArrived(true);
    var latlon = teamLoc.split(",");
    if (latlon[1] == null || latlon[1] == "") return null;
    if (latlon[0] == null || latlon[0] == "") return null;
    var myLatlng = new google.maps.LatLng(latlon[0], latlon[1]);
    var dist = google.maps.geometry.spherical.computeDistanceBetween(acceptTaskMarker.getPosition(), myLatlng);
    if (dist <= 50) {
        app.isArrived(true);
        ArrivedTaskClick();
    }
    else {
        app.isArrived(false);
    }
    // app.isArrived(true);
    // ArrivedTaskClick();
}
function drawTaskMarkers(data) {
    for (var i = 0; i < data.length; i++) {
        var marker1 = placeMarker(data[i].COORDINATE, "Open");
        var strDate = data[i].START;
        strDate = strDate.replace("/Date(", "");
        strDate = strDate.replace(")/", "");
        var startDate = new Date(parseInt(strDate));
        if (marker1 != null) {
            marker1.setTitle(data[i].TASK);
            marker1.info = new google.maps.InfoWindow({
                content: '<b>Task Name:</b> ' + data[i].TASK + '<br><b>Start:</b>' + startDate.toUTCString()
                    + '<br><b>Detail:</b>' + data[i].DETAILS
            });
            google.maps.event.addListener(marker1, 'click', (function (marker1) {
                return function () {
                    document.getElementById("MobileTaskLocation").value = marker1.getPosition().lat() + "," + marker1.getPosition().lng();
                    selectedTaskMarker = marker1;
                    app.taskSelected(marker1.title);
                    for (var j = 0; j < tasksMarkers.length - 1; j++) {
                        tasksMarkers[j].info.close();
                    }
                    marker1.info.open(map, marker1);
                };
            })(marker1));
            tasksMarkers.push(marker1);
        }
    }
    if ($("#MobileTeamLocation").val() != "") {
        tasksMarkers.push(mobileTeamMarker($("#MobileTeamLocation").val(), $("#MobileTeamName").val()));
    }
    //displayTeamChange();
    setFitBound(map);

}

function addTaskMarkers(task) {
    //for (var i = 0; i < data.length; i++) {
    var marker = placeMarker(task.COORDINATE, "Open");

    if (marker != null) {
        marker.setTitle(task.TASK);
        marker.info = new google.maps.InfoWindow({
            content: '<b>Task Name:</b> ' + 'task.TASK' + '<br><b>Start:</b>' + 'task.START'
                + '<br><b>Detail:</b>' + 'task.DETAILS'
        });
        google.maps.event.addListener(marker, 'click', function (marker) {

            document.getElementById("MobileTaskLocation").value = marker.getPosition().lat() + "," + marker.getPosition().lng();
            selectedTaskMarker = marker;
            app.taskSelected(marker.title);
            for (var j = 0; j < tasksMarkers.length - 1; j++) {
                tasksMarkers[j].info.close();
            }
            //marker1.info.open(map, marker1);
            marker.info.open(map, marker);
        });
        tasksMarkers.push(marker);
    }
    setFitBound(map);
}

/////////////////////////////////////track/////////////////////////
function drawTaskTack() {
    //tracktaskList = [];
    document.getElementById("DateList").selectedIndex = -1;

    if (polyRoute != null) polyRoute.setMap(null);
    for (var i = 0; i < trackRouteList.length ; i++) {
        trackRouteList[i].setMap(null);
    }
    trackRouteList = [];
    //tracktaskList.push($('#TaskList').val());
    var param = { taskname: $('#TaskList').val() }
    $.ajax({
        type: "POST",
        url: "getTaskRoutePoints",
        data: param,
        dataType: "text",
        success: function (data) {
            if (data != "") {
                var pts = JSON.parse(data);
                polyRoute = makePolyLine(pts);
                polyRoute.setMap(map);
                trackRouteList.push(polyRoute);
                setRouteFitBound(trackRouteList);
            }
        }
    })
}
function drawDateTaskTack() {
    //tracktaskList = [];
    if (polyRoute != null) polyRoute.setMap(null);
    for (var i = 0; i < trackRouteList.length ; i++) {
        trackRouteList[i].setMap(null);
    }
    document.getElementById("TaskList").selectedIndex = -1;

    var param = { selectedDate: $('#DateList').val() }
    $.ajax({
        type: "POST",
        url: "getDateRoutePoints",
        data: param,
        dataType: "text",
        success: function (data) {
            if (data != "") {
                var polyList = JSON.parse(data);
                for (var j = 0 ; j < polyList.length; j++) {
                    //tracktaskList.push(pts[j][0].taskName);
                    var pts = polyList[j];
                    var polyRoute1 = makePolyLine(pts);
                    polyRoute1.setMap(map);
                    trackRouteList.push[polyRoute1];
                }
                setRouteFitBound(trackRouteList);
            }
        }
    })
}
function setRouteFitBound(trackRouteList) {
    var bounds = new google.maps.LatLngBounds();
    for (var i = 0; i < trackRouteList.length; i++) {
        var points = trackRouteList[i].getPath().getArray();
        for (var n = 0; n < points.length ; n++) {
            bounds.extend(points[n]);
        }
    }
    map.fitBounds(bounds);
}
function makePolyLineMobile(pts) {
    return new google.maps.Polyline({
        path: pts,
        geodesic: true,
        strokeColor: '#ff004c',
        strokeOpacity: 1.0,
        strokeWeight: 1
    });
}
function makePolyLine(pts) {
    var poly = [];
    var pos;
    for (var i = 0; i < pts.length ; i++) {
        if (i == pts[i].index) pos = new google.maps.LatLng(pts[i].lat, pts[i].lng);
        else {
            for (var j = i; j < pts.length; j++) {
                if (i == pts[j].index) {
                    pos = new google.maps.LatLng(pts[j].lat, pts[j].lng);
                    break;
                }
            }
        }

        poly.push(pos);
    }
    return new google.maps.Polyline({
        path: poly,
        geodesic: true,
        strokeColor: '#ff004c',
        strokeOpacity: 1.0,
        strokeWeight: 1
    });
}
function deleteTracks() {
    if (polyRoute != null) polyRoute.setMap(null);
    for (var i = 0; i < trackRouteList.length ; i++) {
        trackRouteList[i].setMap(null);
    }

}
function activeTrackTask(row) {
    // deleteTracks();
    var tdid = row.cells[0];
    var strid = tdid.innerHTML;
    var param = { id: strid }
    $.ajax({
        type: "POST",
        url: "getIdPoints",
        data: param,
        dataType: "text",
        success: function (data) {
            if (data != "") {
                var pts = JSON.parse(data);
                polyRoute = makePolyLine(pts);
                polyRoute.setMap(map);
                trackRouteList.push(polyRoute);
                setRouteFitBound(trackRouteList);
            }
        }
    })
    //mapSetCenter(locationData);
    //alert(taskDataList);
}

function showCmbTeam() {
    if ($("#cmbTeams").children("option").length < 1)
    {
        $("#cmbUserRole").val("Administrator");
        return alert("you need to add team first")
    }
    else if ($("#cmbUserRole").val() != "Administrator") {
        $("#li_Teams").show();
    }
    else {
        $("#li_Teams").hide();
    }
}