/*

A javascript File to encapsulate the Uploading using FILE API to MVC Controller implementing the UploadedFile MODEL

*/
var ImageUploadResultPromise = new $.Deferred();

//Function to generate the NEWS Image file names by concatenating a user entered news Date to a Prefix in the date format ddMMMyyy.
//i.e News-12Feb2012
function createFileName(imgPrefix, pkDate) {
    if (typeof (pkDate) == "undefined") {
        throw new Error("The Date field to use is Undefined");
    }
    var dateString = imgPrefix;
    var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    pkDate = pkDate.replace("a.m.", "").replace("p.m.", "").split(" ");
    year = pkDate[2];
    if (year.length == 2) {
        year = "20" + year
    };
    month = monthNames.indexOf(pkDate[1]);
    day = pkDate[0];
    var newsDate = new Date(year, month, day);
    if (newsDate == "Invalid Date") { throw "Invalid Date (" + pkDate +") for Image formaat"; };
    dateString += newsDate.getDate();
    dateString += (monthNames[newsDate.getMonth()]);
    dateString += newsDate.getFullYear();
    return dateString;
}

//MAIN function to handle the interaction between browser and server for uploading a file
// - Creates and sets up an XMLHTTPequest object
// - Sets the data row cell of .DBIageURIText to the response (relative file name - ../Images/News/...)
// - Sets up a progress report for the user informing them how far the file has been uploaded.
// - Sets up the on Load event to inform user the file upload was successfull.
// - Sets up the Request Headers to pass info about the file to the server
// - Sends the file to the server.
function UpLoadFiles(file, fileName, droplabel, imgUploadParams) {

    //Create XMLHttpRequest
    var xhr = new XMLHttpRequest();

    //Set up event handler for when our asynch call changes state to set any data cells that need to know the images relative path as returned by the server
    xhr.onreadystatechange = function () {
        //We only really care about success
        if (xhr.readyState == 4 && xhr.status == 200) {
            //change preview image to result of the file upload, which is our relaive path on the server
            //alert("success");
            $(droplabel).parents(imgUploadParams.parent).find(imgUploadParams.uriInputSelector).children("input").val(xhr.responseText);
            //$("#ImgURL").attr("value", xhr.responseText);
        };
    };

    xhr.onprogress = function (evt) {
        if (evt.lengthComputable) {
            var percentageUploaded = parseInt((evt.loaded / evt.total) * 100);
            $(droplabel).text(percentageUploaded + "% complete");
        }    
    };

    //Set up the file upload progress info to inform user the percentage uploaded
    xhr.addEventListener("onprogress", function (evt) {

        if (evt.lengthComputable) {
            //alert("Have Upload")
            var percentageUploaded = parseInt((evt.loaded / evt.total) * 100);
            $(droplabel).text(percentageUploaded + "% complete");
        }
    }, true); //CHANGED FROM FALSE to TRUE
    xhr.addEventListener('onerror',function(evt) {
        //imgUploadParams.promise.reject();
        ImageUploadResultPromise.reject('XHR upload failure: ' + evt.message);
        //throw new Error('XHR upload failure: ' + evt.message );
    },false);
    //Set up OnLOAD event to inform user when file successfully uploaded.
    xhr.addEventListener("load", function () {
        //Inform user when file completely loaded
        //How can we send a result back to the calling program to allow it to handle a successfully loaded image??
        if (xhr.status == 200) {
            alert("File Successfully Uploaded to Server");
            var a = $.Deferred();
            //imgUploadParams.promise.resolve();
            ImageUploadResultPromise.resolve();
        } else {
            ImageUploadResultPromise.reject('XHR upload failure: ' + xhr.statusText);
            }
    }, false);

    //Setup Asynch call to server to upload file via POST
    xhr.open("POST", imgUploadParams.postURL, true);

    //Set request headers to pass file information to server
    xhr.setRequestHeader("Content-Type", "multipart/form-data");
    xhr.setRequestHeader("X-File-Name", fileName);
    xhr.setRequestHeader("X-File-Size", file.size);
    xhr.setRequestHeader("X-File-Type", file.type);
    xhr.setRequestHeader("X-File-Path", imgUploadParams.savePathRelative);
    xhr.setRequestHeader("X-File-MainX", imgUploadParams.imgWidth);
    xhr.setRequestHeader("X-File-MainY", imgUploadParams.imgHeight);
    xhr.setRequestHeader("X-File-SubX", imgUploadParams.imgThumbWidth);
    xhr.setRequestHeader("X-File-SubY", imgUploadParams.imgThumbHeight);

    //Begin uploading the file
    xhr.send(file);

};


//Main Function to setup and begin Processing a File Drop event on to our drop box object (dbObject) using the passed in parameters
// - Reads the first file into a reader object and sets the preview source to the image.
// - Sets the filename to save the file based on parameters passed in
// - Checks a filename has been created/set in order to continue
// - Calls UploadFiles function to handle the interaction between browser and server for uploading the file
function handleFiles(files, dbObj, imgUploadParams, dropPreview, droplabel) {
    //Only look at first file, as we don't care about more!
    var file = files[0];

    //Inform user that file is being processed!
    
    droplabel.html("Processing " + file.name);

    var reader = new FileReader();

    // init the reader event handlers
    //var dropPreview = dbObj.parent().children("img#preview");
    reader.onload = (function (evt) {
        // Render thumbnail. This event only happens AFTER the reader is complete
        dropPreview.attr("src",evt.target.result);
        //};
    });

    // begin the read operation
    reader.readAsDataURL(file);

    
    
    var fileName = imgUploadParams.imgPrefix;

    if (imgUploadParams.useDynamicDate) {
        //Find any data value that we might need to concatenate to the file name (For NEWS images)
        var pkDate = imgUploadParams.dateSelector();
        if (pkDate == "") { throw new Error("Date field must be selected first before uploading image");};
        fileName = createFileName(imgUploadParams.imgPrefix, pkDate);
    } else{
        fileName = fileName + file.name;
    }

    //If the filename hasn't been set by now we can't continue.
    if (fileName == null || fileName.length == 0) {
        //alert("You must set " + imgUploadParams.fileNameField + " first!");
        //dbObjcss("background-color", "white");
        throw new Error("Date field must be selected first before uploading image");
        //return false;
    }

    //Handle the file upload between Browser and Server
    UpLoadFiles(file, fileName, droplabel, imgUploadParams);
    return true;
};

//Function to Handle a drop event
// - Stops default event propogation
// - Calls the handleFiles function to process any files passed in.
function dropHandler(evt, dropObj, imgUploadParams) {
    //Reset the Promise...Not sure how this will work for multiple drops..
    //ImageUploadResultPromise = new $.Deferred();
    //Inform the user that the drop event has begun through the imgDropped promise.
    imgUploadParams.imgDropped.resolve(dropObj);
    //Stop default event handling
    evt.stopPropagation();
    evt.preventDefault();
    var dropPreview = dropObj.parent().find("img#preview");
    var droplabel = dropObj.children("span#droplabel");
    //Find the Files that have been dropped
    var files = evt.originalEvent.dataTransfer.files;
    var count = files.length;
    //var pkDate = $("tr td #Date").val();

    // if (pkDate == null || pkDate.length == 0) {
    //    alert("You must set " +fileNameField + " first!");
    //     $("#dropbox").css("background-color", "white");
    //     return false;
    //}

    // Only call the handler if 1 or more files was dropped.
    if (count > 0) {
        try {
            handleFiles(files, dropObj, imgUploadParams,dropPreview, droplabel);
        } catch (e) {
            ImageUploadResultPromise.reject('Pre upload Failure: ' + e.message);
            alert(e.message);
            dropObj.css("background-color", "white");
            dropPreview.attr("src", "/no_image");
            droplabel.html("Erorr Processing file. " + files[0].name);
        }
        return true;
    }
};


//Set the Background color blue when we enter the droppable file area!
function dragenter(evt) {
    evt.stopPropagation();
    evt.preventDefault();

    $(this).css("background-color", "blue");

};

//Set the Background color back to White when we move outside the droppable file area!
function dragexit(evt) {
    evt.stopPropagation();
    evt.preventDefault();

    $(this).css("background-color", "white");

};

//Function to stop Drag&Drop event propogation
function noopHandler(evt) {
    //alert("noop");
    evt.stopPropagation();
    evt.preventDefault();
}

function resetPromise() {
    ImageUploadResultPromise = new $.Deferred();
    return ImageUploadResultPromise;
}

//MAIN PUBLIC FUNCTION CALLED FROM OUTSIDE THIS SCRIPT BY THE PAGE TO SET UP ALL EVENTS
// - Stops MAIN window from accepting drop events as if you drop an image anywhere in browser it will auto open the Image file and move away from our page
// - Sets up events on the drop box container to handle and delegate events to each drop box
function setupDropable(containerClass, imgUploadParams) {


    //Block Main Window from Opening File in the window!!! Only the drop zone can accept files
    window.addEventListener("dragover", function (e) {
        e = e || event;
        e.dataTransfer.dropEffect = "none";
        e.dataTransfer.dropAllowed = "none";
        e.stopPropagation();
        e.preventDefault();
    }, false);


    //var imgUploadParams = {
    //    imgPrefix: imgPrefixIN,
    //    useDynamicCache: useDynamicDateIN,
    //    fileNameField: fileNameFieldIN,
    //    postURL: postURLIN,
    //    savePathRelative: savePathRelativeIN,
    //    imgWidth: imgWidthIN,
    //    imgHeight: imgHeightIN
    //};

    //ImageUploadResultPromise = new $.Deferred();
    //Find Drop File Ares
    //var dropbox = $("#" + containerClass + " .dropbox");

    
    // init event handlers
    //dropbox.bind("dragenter", dragenter);
    //dropbox.bind("dragexit", dragexit);
    //dropbox.bind("dragover", noopHandler);
    //dropbox.bind("drop", function (event) { dropHandler(event, imgUploadParams); });

    //The dropbox is the actual UNIQUE CONTAINER that contains a set of one or more drop boxes
    var dropbox = $("#" + containerClass);

    //The dropboxSelecter is the CLASS of drop box that will allow drop events
    var dropboxSelecter = ".dropbox";

    //Set up the CONTAINER to handle any drop events and delegate them to the appropriate DROP BOX SELECTER
    //This allows for adding/removing individual drop box areas dynamically
    dropbox.delegate(dropboxSelecter, "dragenter", dragenter);
    dropbox.delegate(dropboxSelecter, "dragexit", dragexit);
    dropbox.delegate(dropboxSelecter, "dragover", noopHandler);
    //The MAIN drop event handler
    dropbox.delegate(dropboxSelecter, "drop", function (event) { dropHandler(event, $(this), imgUploadParams); });

    //NOTE that this is only the PROMISE! IT WILL GET RESOLVED AT SOME LATER STAGE
    return ImageUploadResultPromise;
};