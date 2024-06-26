﻿// Get the logout button
var logoutButton = document.getElementById('logoutLink');

// If the logout button exists, attach the event listener
if (logoutButton) {
    logoutButton.addEventListener("click", function (event) {
        showLoadingScreen()
        event.preventDefault(); // Prevent the default link behavior

        // Obtain the anti-forgery token from the form
        var token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        // Create the headers with the anti-forgery token
        var headers = {
            "Content-Type": "application/json",
            "RequestVerificationToken": token
        };

        // Send an AJAX POST request to the logout endpoint
        fetch("/Account/Logout", {
            method: "POST",
            headers: headers
        })
            .then(function (response) {
                if (response.ok) {
                    hideLoadingScreen();
                    // Successful logout, redirect to the desired page
                    window.location.href = "/";
                } else {
                    hideLoadingScreen();
                    // Handle error response if needed
                    console.error("Logout failed");
                }
            })
            .catch(function (error) {
                hideLoadingScreen();
                // Handle network or other errors
                console.error("An error occurred during logout:", error);
            });
    });
}
// Include the lottie-web library. You can also use a CDN, install via npm or yarn, or include it in your build process

const animation = lottie.loadAnimation({
    container: document.getElementById('lottie-animation'),
    renderer: 'svg',
    loop: true,
    autoplay: false,
    path: '/documents/Loading-animation-Static.json'
});
function showLoadingScreen() {
    // Enable loading screen
    document.getElementById('lottie-animation').style.visibility = 'visible';

    // Start the animation
    animation.play();

    // Disable all buttons on the page
    const buttons = document.querySelectorAll('button');
    buttons.forEach(button => button.disabled = true);

    // Disable scrolling
    document.body.style.overflow = 'hidden';
}

function hideLoadingScreen() {
    // Stop the animation
    animation.stop();

    // Disable loading screen
    document.getElementById('lottie-animation').style.visibility = 'hidden';

    // Enable all buttons on the page
    const buttons = document.querySelectorAll('button');
    buttons.forEach(button => button.disabled = false);

    // Enable scrolling
    document.body.style.overflow = 'auto';
}

var submitButton =document.getElementById('registerBtn');

// If the submit button exists, attach the event listener
if (submitButton) {
    submitButton.addEventListener("click", function (event) {
        // Prevent the default submit behavior
        event.preventDefault();

        // Show loading screen
        showLoadingScreen();

        // Submit the form manually
        event.target.form.submit();
    });
}

if ($("body").data("title") === "Rates") {

    var ViewModel = function () {

        var self = this;
        //defines an observableArray named rates, kina like var rates = new List<Rates>()
        self.rates = ko.observableArray();
        self.hasScrolled = false;
        //defines a requestForm Object w observables placed in inputs to retrieve the value
        self.requestForm = ko.observable({
                FromZip: ko.observable(),
                ToZip: ko.observable(),
                Length: ko.observable(),
                Width: ko.observable(),
                Height: ko.observable(),
                Pounds: ko.observable(),
                Ounces: ko.observable(),
                Weight: ko.observable(),
                zeroValueMessage: ko.observable("")
        });
        var madeWeight = false;
        isOverweight = function () {

            var p = parseFloat(self.requestForm().Pounds());
            var o = parseFloat(self.requestForm().Ounces());
            if (isNaN(o)) {
                o = 0;
            }

            var total = p + (o / 16);
            if (p > 150 || total > 150) {
                document.getElementById("weightError").innerHTML = "Maximum weight is 150 lbs, you entered: " + p + " lbs and " + o + " oz. Total of: " + total.toFixed(2) + " lbs";
                madeWeight = false;
            }
            else {
                document.getElementById("weightError").innerHTML = "";
                madeWeight = true;
                var w = total.toFixed(2);
                self.requestForm().Weight(Math.ceil(w));
                var after = self.requestForm().Weight();
            }

        };
        self.zeroValueMessage = ko.observable("");

        self.isOverLimit = ko.computed(function () {
            var length = parseFloat(self.requestForm().Length()) || 0;
            var width = parseFloat(self.requestForm().Width()) || 0;
            var height = parseFloat(self.requestForm().Height()) || 0;

            var girth = (width * 2) + (height * 2);
            var result = length + girth;

            return result > 165;
        });

        self.checkValues = function () {
            var length = parseFloat(self.requestForm().Length()) || 0;
            var width = parseFloat(self.requestForm().Width()) || 0;
            var height = parseFloat(self.requestForm().Height()) || 0;

            var zeroValues = [];
            if (length === 0) zeroValues.push("Length");
            if (width === 0) zeroValues.push("Width");
            if (height === 0) zeroValues.push("Height");

            if (zeroValues.length) {
                self.requestForm().zeroValueMessage(zeroValues.join(", ") + " cannot be 0.");
            } else {
                self.requestForm().zeroValueMessage("");
            }
        };


        self.scrollToRates = function () {
            var rateContainer = document.getElementById("rateContainer");
            if (rateContainer) {
                rateContainer.scrollIntoView({ behavior: "smooth", block: "start" });
            }
        };




        self.error = ko.observable();

        self.sendRequest = function (formElement) {
            showLoadingScreen();
            var request = {
                FromZip: self.requestForm().FromZip(),
                ToZip: self.requestForm().ToZip(),
                Length: self.requestForm().Length(),
                Width: self.requestForm().Width(),
                Height: self.requestForm().Height(),
                Weight: self.requestForm().Weight()
            };

            $.ajax({
                type: 'POST',
                url: "/Home/requestRates/",
                dataType: 'json',
                data: request,
                success: function (data) {
                    // hides the loading screen once data has been received
                    hideLoadingScreen();

                    // sends it to the rates variable which is a List<Rates> which have
                    // the values data-binded to div block values in the html 
                    self.rates(data)
                    self.scrollToRates();
                },
                error: function () {
                    // hides the loading screen if there was an error with the request
                    hideLoadingScreen();
                }
            });
        }
    };

    ko.applyBindings(new ViewModel());
}
//requestForm -> sendRequest(requestForm)

if ($("body").data("title") === "Ship") {
   
   
    var ViewModel = function () {

        var self = this;
        var now = new Date();
        self.hasScrolled = false;
        var date = now.toLocaleDateString();
        shouldShow= ko.observable(false);
        canBuy = ko.observable(false);
        var labelID = 0;
        self.userBalance = ko.observable();
        self.requestForm = ko.observable({
            LabelId: ko.observable(),
            ToEmail: ko.observable(),
            ToPhone: ko.observable(),
            ToName: ko.observable(),
            ToCompany: ko.observable(),
            ToAddress1: ko.observable(),
            ToAddress2: ko.observable(),
            ToCity: ko.observable(),
            ToState: ko.observable(),
            ToZip: ko.observable(),
            FromName: ko.observable(),
            FromCompany: ko.observable(),
            FromAddress1: ko.observable(),
            FromAddress2: ko.observable(),
            FromCity: ko.observable(),
            FromState: ko.observable(),
            FromZip: ko.observable(),
            FromPhone: ko.observable(),
            IsReturnAddress: ko.observable(true),
            Length: ko.observable(),
            Width: ko.observable(),
            Height: ko.observable(),
            Pounds: ko.observable(),
            Ounces: ko.observable(),
            Weight: ko.observable(),
            zeroValueMessage: ko.observable("")
        });
        var madeWeight = false;
        isOverweight = function () {
            var pp = self.requestForm().Pounds();
            var p = parseFloat(pp);
            var o = parseFloat(self.requestForm().Ounces());
            if (isNaN(o)) {
                o = 0;
            }

            var total = p + (o / 16);
            if (p > 150 || total > 150) {
                document.getElementById("weightError").innerHTML = "Maximum weight is 150 lbs, you entered: " + p + " lbs and " + o + " oz. Total of: " + total.toFixed(2) + " lbs";
                madeWeight = false;
                shouldShow(false);
            }
            else {
                document.getElementById("weightError").innerHTML = "";
                madeWeight = true;
                var w = total.toFixed(2);
                self.requestForm().Weight(Math.ceil(w));
                var after = self.requestForm().Weight();
                shouldShow(true);
            }

        };
        var selectedLabelId = sessionStorage.getItem("selectedLabelId");
        if (selectedLabelId) {
            $.ajax({
                type: 'GET',
                url: "/Dashboard/GetUnfinishedLabel",
                dataType: 'json',
                data: { labelId: selectedLabelId },
                success: function (data) {
                    self.requestForm().LabelId(selectedLabelId);
                    self.requestForm().ToEmail(data.toEmail);
                    self.requestForm().ToPhone(data.toPhone);
                    self.requestForm().ToName(data.toName);
                    self.requestForm().ToCompany(data.toCompany);
                    self.requestForm().ToAddress1(data.toAddress1);
                    self.requestForm().ToAddress2(data.toAddress2);
                    self.requestForm().ToCity(data.toCity);
                    self.requestForm().ToState(data.toState);
                    self.requestForm().ToZip(data.toZip);
                    self.requestForm().FromName(data.fromName);
                    self.requestForm().FromCompany(data.fromCompany);
                    self.requestForm().FromAddress1(data.fromAddress1);
                    self.requestForm().FromAddress2(data.fromAddress2);
                    self.requestForm().FromCity(data.fromCity);
                    self.requestForm().FromState(data.fromState);
                    self.requestForm().FromZip(data.fromZip);
                    self.requestForm().FromPhone(data.fromPhone);
                    self.requestForm().Length(data.length);
                    self.requestForm().Width(data.width);
                    self.requestForm().Height(data.height);
                    self.requestForm().Pounds(data.weight);
                }
            });

            
            // Remove the selectedLabelId from sessionStorage to avoid using it again unintentionally
            sessionStorage.removeItem("selectedLabelId");
        }
        self.beingPurchased = ko.observable(false);
        //rates is a List<RateDTO>
        self.rates = ko.observableArray();

        //selectedrate is a RateDTO
        self.selectedrate = ko.observable();
        self.createLabelObject = ko.observable({
            selectedOrder: ko.observable(),
            selectedClass: ko.observable()
        });
        self.Item = ko.observable({
            selectedOrder: ko.observable(),
            selectedClass: ko.observable()
        });
        self.labelRequest = ko.observable(
            {

                beginningBalance: ko.observable(),
                totalCost: ko.observable(),
                totalCharge: ko.observable(),
                endingBalance: ko.observable(),

                currentDate: ko.computed(() => {
                    const date = new Date();
                    const monthNames = [
                        "January", "February", "March",
                        "April", "May", "June", "July",
                        "August", "September", "October",
                        "November", "December"
                    ];

                    const month = monthNames[date.getMonth()];
                    const day = date.getDate();
                    const year = date.getFullYear();

                    const ordinal = day % 10 === 1 && day !== 11
                        ? "st"
                        : day % 10 === 2 && day !== 12
                            ? "nd"
                            : day % 10 === 3 && day !== 13
                                ? "rd"
                                : "th";

                    return `${month} ${day}${ordinal}, ${year}`;
                })
            }
        );
        var isChargedEnough = false;
        getUserBalance = function () {
            $.ajax({
                type: 'POST',
                url: "/Home/GetUserBalance/",
                dataType: 'text', // Set the response data type as text
                success: function (userBalance) {
                    var formattedBalance = parseFloat(userBalance).toFixed(2); // Parse and format the balance value
                    self.labelRequest().beginningBalance(formattedBalance);
                    calculateBalance();
                },
                error: function (xhr, status, error) {
                    // Handle the error here
                    console.error('Error occurred while retrieving user balance:', error);
                    // You can show an error message to the user or perform any other error handling logic
                }
            });
        }

        calculateBalance = function () {
            const div = document.getElementById("endingBalanceColumn");
            const chargeDifference = parseFloat(self.labelRequest().totalCharge()) - parseFloat(self.labelRequest().totalCost());
            if (isNaN(chargeDifference)) {
                const beginningBalance = parseFloat(self.labelRequest().beginningBalance());
                if (beginningBalance >= parseFloat(self.labelRequest().totalCost())) {
                   
                    div.style.backgroundColor = "rgba(8, 131, 35, 0.66)";
                    isChargedEnough = true;
                    canBuy(true);
                } else {
                    const endingBalance = beginningBalance - parseFloat(self.labelRequest().totalCost());
                    self.labelRequest().endingBalance(endingBalance.toFixed(2));
                    div.style.backgroundColor = "#F17A69";
                    isChargedEnough = false;
                    canBuy(false);
                }
            } else {
                var endingB = parseFloat(self.labelRequest().beginningBalance()) + chargeDifference;
                var roundedEnding = (endingB * 100) / 100;

                if (roundedEnding < 0) {
                    div.style.backgroundColor = "#F17A69";
                    isChargedEnough = false;
                    canBuy(false);
                } else {
                    div.style.backgroundColor = "rgba(8, 131, 35, 0.66)";
                    isChargedEnough = true;
                    canBuy(true);
                }
                self.labelRequest().endingBalance(roundedEnding.toFixed(2)); // Format the endingBalance value
            }
        }


    
      
       
        self.zeroValueMessage = ko.observable("");

        self.isOverLimit = ko.computed(function () {
            var length = parseFloat(self.requestForm().Length()) || 0;
            var width = parseFloat(self.requestForm().Width()) || 0;
            var height = parseFloat(self.requestForm().Height()) || 0;

            var girth = (width * 2) + (height * 2);
            var result = length + girth;
            var isOver = result > 165;
            shouldShow(!isOver);
            return isOver;
        });

        self.checkValues = function () {
            var length = parseFloat(self.requestForm().Length()) || 0;
            var width = parseFloat(self.requestForm().Width()) || 0;
            var height = parseFloat(self.requestForm().Height()) || 0;

            var zeroValues = [];
            if (length === 0) zeroValues.push("Length");
            if (width === 0) zeroValues.push("Width");
            if (height === 0) zeroValues.push("Height");

            if (zeroValues.length) {
                self.requestForm().zeroValueMessage(zeroValues.join(", ") + " cannot be 0.");
                shouldShow(false);
            } else {
                self.requestForm().zeroValueMessage("");
                shouldShow(true);
            }
        };
        
        self.error = ko.observable();
        $.ajax({
            type: 'GET',
            url: "/Home/getUserDeets/",
            dataType: 'json',
            success: function (data) {
                self.requestForm().FromName(data.fullName);
                self.requestForm().FromPhone(data.phone);

            }
        });

        $.ajax({
            type: 'GET',
            url: "/Home/getUserAddress/",
            dataType: 'json',
            success: function (data) {
                if (data != undefined) {
                    self.requestForm().FromAddress1(data.addressLine1);
                    self.requestForm().FromAddress2(data.addressLine2);
                    self.requestForm().FromCity(data.city);
                    self.requestForm().FromState(data.stateProvinceCode);
                    self.requestForm().FromZip(data.postalCode);
                    self.requestForm().FromCompany(data.company);
                }

            }
        });

        self.scrollToRates = function () {
            var rateContainer = document.getElementById("dropdownrates");
            if (rateContainer) {
                rateContainer.scrollIntoView({ behavior: "smooth", block: "start" });
            }
        };

        function addressAutocomplete(containerElement, callback, options) {

            const MIN_ADDRESS_LENGTH = 5;
            const DEBOUNCE_DELAY = 100;

            // create container for input element
            const inputContainerElement = document.createElement("div");
            inputContainerElement.setAttribute("class", "input-container");
            containerElement.appendChild(inputContainerElement);

            // create input element
            const inputElement = document.createElement("input");
            inputElement.setAttribute("type", "text");
            inputElement.setAttribute("data-bind", "value: ToAddress1");
            inputElement.setAttribute("placeholder", "Address");
            inputElement.setAttribute("id", "ToAddress1");
            inputElement.setAttribute("name", "ToAddress1");
            inputElement.setAttribute("class", "input-field w-input");
            inputContainerElement.appendChild(inputElement);

            // add input field clear button
            const clearButton = document.createElement("div");
            clearButton.classList.add("clear-button");
            addIcon(clearButton);
            clearButton.addEventListener("click", (e) => {
                e.stopPropagation();
                inputElement.value = '';
                callback(null);
                clearButton.classList.remove("visible");
                closeDropDownList();
            });
            inputContainerElement.appendChild(clearButton);

            /* We will call the API with a timeout to prevent unneccessary API activity.*/
            let currentTimeout;

            /* Save the current request promise reject function. To be able to cancel the promise when a new request comes */
            let currentPromiseReject;

            /* Focused item in the autocomplete list. This variable is used to navigate with buttons */
            let focusedItemIndex;

            /* Process a user input: */
            inputElement.addEventListener("input", function (e) {
                const currentValue = this.value;

                /* Close any already open dropdown list */
                closeDropDownList();


                // Cancel previous timeout
                if (currentTimeout) {
                    clearTimeout(currentTimeout);
                }

                // Cancel previous request promise
                if (currentPromiseReject) {
                    currentPromiseReject({
                        canceled: true
                    });
                }

                if (!currentValue) {
                    clearButton.classList.remove("visible");
                }

                // Show clearButton when there is a text
                clearButton.classList.add("visible");

                // Skip empty or short address strings
                if (!currentValue || currentValue.length < MIN_ADDRESS_LENGTH) {
                    return false;
                }

                /* Call the Address Autocomplete API with a delay */
                currentTimeout = setTimeout(() => {
                    currentTimeout = null;

                    /* Create a new promise and send geocoding request */
                    const promise = new Promise((resolve, reject) => {
                        currentPromiseReject = reject;

                        // The API Key provided is restricted to JSFiddle website
                        // Get your own API Key on https://myprojects.geoapify.com
                        const apiKey = "746416c89e4f40bab5b4e390ce4a1632";

                        var url = `https://api.geoapify.com/v1/geocode/autocomplete?text=${encodeURIComponent(currentValue)}&format=json&limit=5&apiKey=${apiKey}`;

                        fetch(url)
                            .then(response => {
                                currentPromiseReject = null;

                                // check if the call was successful
                                if (response.ok) {
                                    response.json().then(data => resolve(data));
                                } else {
                                    response.json().then(data => reject(data));
                                }
                            });
                    });

                    promise.then((data) => {
                        // here we get address suggestions
                        currentItems = data.results;

                        /*create a DIV element that will contain the items (values):*/
                        const autocompleteItemsElement = document.createElement("div");
                        autocompleteItemsElement.setAttribute("class", "autocomplete-items");
                        inputContainerElement.appendChild(autocompleteItemsElement);

                        /* For each item in the results */
                        data.results.forEach((result, index) => {
                            /* Create a DIV element for each element: */
                            const itemElement = document.createElement("div");
                            /* Set formatted address as item value */
                            itemElement.innerHTML = result.formatted;
                            autocompleteItemsElement.appendChild(itemElement);

                            /* Set the value for the autocomplete text field and notify: */
                            itemElement.addEventListener("click", function (e) {
                                inputElement.value = currentItems[index].address_line1;
                                if ('' + currentItems[index].address_line1 + '' != 'undefined') {
                                    self.requestForm().ToAddress1(currentItems[index].address_line1);

                                } 
                                if ('' + currentItems[index].state_code + '' != 'undefined') {
                                    self.requestForm().ToState(currentItems[index].state_code);

                                }
                                if ('' + currentItems[index].city + '' != 'undefined') {
                                    self.requestForm().ToCity(currentItems[index].city);

                                }
                                if ('' + currentItems[index].postcode + '' != 'undefined') {
                                    self.requestForm().ToZip(currentItems[index].postcode);
                                }
                                callback(currentItems[index]);
                                /* Close the list of autocompleted values: */
                                closeDropDownList();
                            });
                        });

                    }, (err) => {
                        if (!err.canceled) {
                            console.log(err);
                        }
                    });
                }, DEBOUNCE_DELAY);
            });

            /* Add support for keyboard navigation */
            inputElement.addEventListener("keydown", function (e) {
                var autocompleteItemsElement = containerElement.querySelector(".autocomplete-items");
                if (autocompleteItemsElement) {
                    var itemElements = autocompleteItemsElement.getElementsByTagName("div");
                    if (e.keyCode == 40) {
                        e.preventDefault();
                        /*If the arrow DOWN key is pressed, increase the focusedItemIndex variable:*/
                        focusedItemIndex = focusedItemIndex !== itemElements.length - 1 ? focusedItemIndex + 1 : 0;
                        /*and and make the current item more visible:*/
                        setActive(itemElements, focusedItemIndex);
                    } else if (e.keyCode == 38) {
                        e.preventDefault();

                        /*If the arrow UP key is pressed, decrease the focusedItemIndex variable:*/
                        focusedItemIndex = focusedItemIndex !== 0 ? focusedItemIndex - 1 : focusedItemIndex = (itemElements.length - 1);
                        /*and and make the current item more visible:*/
                        setActive(itemElements, focusedItemIndex);
                    } else if (e.keyCode == 13) {
                        /* If the ENTER key is pressed and value as selected, close the list*/
                        e.preventDefault();
                        if (focusedItemIndex > -1) {
                            closeDropDownList();
                        }
                    }
                } else {
                    if (e.keyCode == 40) {
                        /* Open dropdown list again */
                        var event = document.createEvent('Event');
                        event.initEvent('input', true, true);
                        inputElement.dispatchEvent(event);
                    }
                }
            });

            function setActive(items, index) {
                if (!items || !items.length) return false;

                for (var i = 0; i < items.length; i++) {
                    items[i].classList.remove("autocomplete-active");
                }

                /* Add class "autocomplete-active" to the active element*/
                items[index].classList.add("autocomplete-active");

                // Change input value and notify
                inputElement.value = currentItems[index].formatted;
                callback(currentItems[index]);
            }

            function closeDropDownList() {
                const autocompleteItemsElement = inputContainerElement.querySelector(".autocomplete-items");
                if (autocompleteItemsElement) {
                    inputContainerElement.removeChild(autocompleteItemsElement);
                }

                focusedItemIndex = -1;
            }

            function addIcon(buttonElement) {
                const svgElement = document.createElementNS("http://www.w3.org/2000/svg", 'svg');
                svgElement.setAttribute('viewBox', "0 0 24 24");
                svgElement.setAttribute('height', "24");

                const iconElement = document.createElementNS("http://www.w3.org/2000/svg", 'path');
                iconElement.setAttribute("d", "M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z");
                iconElement.setAttribute('fill', 'currentColor');
                svgElement.appendChild(iconElement);
                buttonElement.appendChild(svgElement);
            }

            /* Close the autocomplete dropdown when the document is clicked. 
              Skip, when a user clicks on the input field */
            document.addEventListener("click", function (e) {
                if (e.target !== inputElement) {
                    closeDropDownList();
                } else if (!containerElement.querySelector(".autocomplete-items")) {
                    // open dropdown list again
                    var event = document.createEvent('Event');
                    event.initEvent('input', true, true);
                    inputElement.dispatchEvent(event);
                }
            });
        }

        addressAutocomplete(document.getElementById("autocomplete-container"), (data) => {
           
        }, {
            placeholder: "Enter an address here"
        });

        self.sendRequest = function (formElement) {
            showLoadingScreen();
            isOverweight()
            //document.getElementById("msgDiv").style.display = "block";
            if (madeWeight) {
                var request = {
                    LabelId: self.requestForm().LabelId(),
                    ToEmail: self.requestForm().ToEmail(),
                    ToPhone: self.requestForm().ToPhone(),
                    ToName: self.requestForm().ToName(),
                    ToCompany: self.requestForm().ToCompany(),
                    ToAddress1: self.requestForm().ToAddress1(),
                    ToAddress2: self.requestForm().ToAddress2(),
                    ToCity: self.requestForm().ToCity(),
                    ToState: self.requestForm().ToState(),
                    ToZip: self.requestForm().ToZip(),

                    FromName: self.requestForm().FromName(),
                    FromCompany: self.requestForm().FromCompany(),
                    FromAddress1: self.requestForm().FromAddress1(),
                    FromAddress2: self.requestForm().FromAddress2(),
                    FromCity: self.requestForm().FromCity(),
                    FromState: self.requestForm().FromState(),
                    FromZip: self.requestForm().FromZip(),
                    FromPhone: self.requestForm().FromPhone(),
                    IsReturnAdress: self.requestForm().IsReturnAddress(),
                    
                    Length: self.requestForm().Length(),
                    Width: self.requestForm().Width(),
                    Height: self.requestForm().Height(),
                    Weight: self.requestForm().Weight()
                };
                self.createLabelObject().selectedOrder(request);
                if (request.IsReturnAdress) {
                    var addy = {
                        AddressLine1: request.FromAddress1,
                        AddressLine2: request.FromAddress2,
                        Company: request.FromCompany,
                        City: request.FromCity,
                        StateProvinceCode: request.FromState,
                        PostalCode: request.FromZip,
                        IsReturnAddress: request.IsReturnAddress
                    };

                    $.ajax({
                        type: 'POST',
                        url: "/Dashboard/UpdateAddress/",
                        data: addy
                    });
                }







                $.ajax({
                    type: 'POST',
                    url: "/Home/GetFullRates/",
                    dataType: 'json',
                    data: request,
                    //data is a FullRateDTO
                    success: function (data) {
                        hideLoadingScreen();
                        if (data.isError == true) {
                            alert(data.error)
                        }
                        else {
                            document.getElementById("dropdownrates").style.display = "block";
                            self.selectedrate(data.selectedrate)
                            self.createLabelObject().selectedClass(data.selectedrate.serviceClass)
                            self.rates(data.rates)
                            self.labelRequest().totalCost(data.selectedrate.ourPrice);
                            self.labelRequest().totalCharge(data.selectedrate.ourPrice);
                            getUserBalance();
                            calculateBalance();
                            labelID = data.upsOrderDetailsId;
                            self.scrollToRates();

                        }
                    },
                    error: function () {
                        // hides the loading screen if there was an error with the request
                        hideLoadingScreen();
                    }
                });
            } else if (!madeWeight) {
                hideLoadingScreen();
                alert("Please enter a valid weight.")
            }
            
        }
        self.selectRate = function (rate) {
            const og = self.selectedrate();
            const newRate = rate;
            self.rates.replace(newRate, og)
            self.selectedrate(newRate)
            self.labelRequest().totalCost(newRate.ourPrice);
            self.labelRequest().totalCharge(newRate.ourPrice);
            self.createLabelObject().selectedClass(newRate.serviceClass);
            calculateBalance()
        };
    
        
        $("#buyLabel").click(function (event) {
            //event.preventDefault();
            // document.getElementById("msgDiv").style.display = "block";
            // change isCharged to hasPayed later
            const items = [
                {
                    order: self.createLabelObject().selectedOrder(),
                    serviceClass: self.createLabelObject().selectedClass(),
                    selectedrate: self.selectedrate()
                }
            ];
          
            if (isChargedEnough) {
              
                self.beingPurchased(true);
                
                // This is your test publishable API key.
                
                 //The items the customer wants to buy
                
               
                function calculateOrderAmount(amount) {
                    const price = parseFloat(amount);

                    if (!isNaN(price)) {
                        // Convert the price to the smallest currency unit (e.g., cents)
                        const smallestCurrencyUnit = Math.round(price * 100);
                        return smallestCurrencyUnit;
                    } else {
                        return 0;
                    }
                }
             
           
                //currently not able to get the Client Secret from fetch below, check errors and openai
               // const stripe = Stripe("pk_test_51MxFCnDHpayIZlcAytKURkjtSmxLNLAd0V2noxps5R1Of0zyHxD67diq4jeehDxzSW2TbyC7Wpu8gDpGi6ros1vU009J6Nf8zm");
                const stripe = Stripe("pk_live_51MxFCnDHpayIZlcAomIsaHnFuJDxnQJxJtQf58k2XzvoK2ZRT5Qwmbam93JzOOcS5HZsuQtZP8dMLU7ac0SsZsz5005fSBdzpr");

                const options = {
                    mode: 'payment',
                    amount: calculateOrderAmount(self.labelRequest().totalCharge()),
                    currency: 'usd',
                    // Fully customizable with appearance API.
                    appearance: {
                        theme: 'night',
                        variables: {
                            colorPrimary: '#827ded',
                            colorBackground: '#171717',
                        },
                    },
                };
                const elements = stripe.elements(options);
                const paymentElement = elements.create('payment');
                paymentElement.mount('#payment-element');


                const form = document.getElementById('payment-form');
                const submitBtn = document.getElementById('submit');

                const handleError = (error) => {
                    const messageContainer = document.querySelector('#error-message');
                    messageContainer.textContent = error.message;
                    submitBtn.disabled = false;
                }

                form.addEventListener('submit', async (event) => {
                    // We don't want to let default form submission happen here,
                    // which would refresh the page.
                    event.preventDefault();

                    // Prevent multiple form submissions
                    if (submitBtn.disabled) {
                        return;
                    }

                    // Disable form submission while loading
                    submitBtn.disabled = true;

                    // Trigger form validation and wallet collection
                    const { error: submitError } = await elements.submit();
                    if (submitError) {
                        handleError(submitError);
                        return;
                    }

                    // Create the PaymentIntent and obtain clientSecret
                    const res = await fetch("/create-intent", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify({
                            amount: calculateOrderAmount(self.selectedrate().ourPrice),
                            charged: calculateOrderAmount(self.labelRequest().totalCharge()),
                            description: labelID,
                            serviceClass: self.createLabelObject().selectedClass(),
                            rateDto: self.selectedrate()
                            
                        })
                    });

                    const { client_secret: clientSecret } = await res.json();

                    // Confirm the PaymentIntent using the details collected by the Payment Element
                    const returnUrl = `${window.location.origin}/Dashboard/PaymentProcessing/`;

                    const { error } = await stripe.confirmPayment({
                        elements,
                        clientSecret,
                        confirmParams: {
                            return_url: returnUrl,
                        },
                    });

                    if (error) {
                        // This point is only reached if there's an immediate error when
                        // confirming the payment. Show the error to your customer (for example, payment details incomplete)
                        handleError(error);
                    } else {
                        // Your customer is redirected to your `return_url`. For some payment
                        // methods like iDEAL, your customer is redirected to an intermediate
                        // site first to authorize the payment, then redirected to the `return_url`.
                    }
                });



            }
        });

      
        

        self.makeTheLabel = function () {
            var root = {
                Rate: self.selectedrate(),
                LabelId: labelID
            }
            $.ajax({
                url: "/Home/makeTheLabel/",
                type: "POST",
                data: root,
                success: function (response) {
                    window.location.href = response.redirectToUrl;
                }
            });
            return false;
        } 
     
    }
    ko.applyBindings(new ViewModel());
   
}

if ($("body").data("title") === "BulkShip") {
   
   
    var ViewModel = function () {

        var self = this;
        var now = new Date();
        self.hasScrolled = false;
        var date = now.toLocaleDateString();
        shouldShow= ko.observable(false);
        canBuy = ko.observable(false);
        var labelID = 0;
        self.userBalance = ko.observable();
        self.overCount = ko.observable();
        self.underCount = ko.observable();
        self.requestForm = ko.observable({
            LabelId: ko.observable(),
            ToEmail: ko.observable(),
            ToPhone: ko.observable(),
            ToName: ko.observable(),
            ToCompany: ko.observable(),
            ToAddress1: ko.observable(),
            ToAddress2: ko.observable(),
            ToCity: ko.observable(),
            ToState: ko.observable(),
            ToZip: ko.observable(),
            FromName: ko.observable(),
            FromCompany: ko.observable(),
            FromAddress1: ko.observable(),
            FromAddress2: ko.observable(),
            FromCity: ko.observable(),
            FromState: ko.observable(),
            FromZip: ko.observable(),
            FromPhone: ko.observable(),
            IsReturnAddress: ko.observable(true),
            Length: ko.observable(),
            Width: ko.observable(),
            Height: ko.observable(),
            Pounds: ko.observable(),
            Ounces: ko.observable(),
            Weight: ko.observable(),
            zeroValueMessage: ko.observable("")
        });
        var madeWeight = false;

        self.ratesOver70 = ko.observableArray();
        self.ratesUnder70 = ko.observableArray();
       
       
        self.selectedRateOver70 = ko.observable();
        self.selectedRateUnder70 = ko.observable();

        self.createLabelObjectOver70 = ko.observable({
            selectedOrder: ko.observable(),
            selectedClass: ko.observable()
        });

        self.createLabelObjectUnder70 = ko.observable({
            selectedOrder: ko.observable(),
            selectedClass: ko.observable()
        });

        self.labelRequestOver70 = ko.observable({
            totalCost: ko.observable(),
            totalCharge: ko.observable()
        });

        self.labelRequestUnder70 = ko.observable({
            totalCost: ko.observable(),
            totalCharge: ko.observable()
        });

        self.labelRequest = ko.observable(
            {

                beginningBalance: ko.observable(),
                totalCost: ko.observable(),
                totalCharge: ko.observable(),
                endingBalance: ko.observable(),

                currentDate: ko.computed(() => {
                    const date = new Date();
                    const monthNames = [
                        "January", "February", "March",
                        "April", "May", "June", "July",
                        "August", "September", "October",
                        "November", "December"
                    ];

                    const month = monthNames[date.getMonth()];
                    const day = date.getDate();
                    const year = date.getFullYear();

                    const ordinal = day % 10 === 1 && day !== 11
                        ? "st"
                        : day % 10 === 2 && day !== 12
                            ? "nd"
                            : day % 10 === 3 && day !== 13
                                ? "rd"
                                : "th";

                    return `${month} ${day}${ordinal}, ${year}`;
                })
            }
        );
        getUserBalance = function () {
            $.ajax({
                type: 'POST',
                url: "/Home/GetUserBalance/",
                dataType: 'text', // Set the response data type as text
                success: function (userBalance) {
                    var formattedBalance = parseFloat(userBalance).toFixed(2); // Parse and format the balance value
                    self.labelRequest().beginningBalance(formattedBalance);
                    calculateBalance();
                },
                error: function (xhr, status, error) {
                    // Handle the error here
                    console.error('Error occurred while retrieving user balance:', error);
                    // You can show an error message to the user or perform any other error handling logic
                }
            });
        }
        calculateBalance = function () {
            const div = document.getElementById("endingBalanceColumn");

            // Add the total costs of both requests
            const totalCostOver70 = parseFloat(self.labelRequestOver70().totalCost());
            const totalCostUnder70 = parseFloat(self.labelRequestUnder70().totalCost());
            const totalCost = totalCostOver70 + totalCostUnder70;
            self.labelRequest().totalCost(totalCost);
            // Add the total charges of both requests
            const totalChargeOver70 = parseFloat(self.labelRequestOver70().totalCharge());
            const totalChargeUnder70 = parseFloat(self.labelRequestUnder70().totalCharge());
            const totalCharge = totalChargeOver70 + totalChargeUnder70;
            self.labelRequest().totalCharge(totalCharge);
            const chargeDifference = totalCharge - totalCost;

            if (isNaN(chargeDifference)) {
                const beginningBalance = parseFloat(self.labelRequest().beginningBalance());
                if (beginningBalance >= totalCost) {
                    div.style.backgroundColor = "rgba(8, 131, 35, 0.66)";
                    isChargedEnough = true;
                    canBuy(true);
                } else {
                    const endingBalance = beginningBalance - totalCost;
                    self.labelRequest().endingBalance(endingBalance.toFixed(2));
                    div.style.backgroundColor = "#F17A69";
                    isChargedEnough = false;
                    canBuy(false);
                }
            } else {
                var endingB = parseFloat(self.labelRequest().beginningBalance()) + chargeDifference;
                var roundedEnding = (endingB * 100) / 100;

                if (roundedEnding < 0) {
                    div.style.backgroundColor = "#F17A69";
                    isChargedEnough = false;
                    canBuy(false);
                } else {
                    div.style.backgroundColor = "rgba(8, 131, 35, 0.66)";
                    isChargedEnough = true;
                    canBuy(true);
                }
                self.labelRequest().endingBalance(roundedEnding.toFixed(2)); // Format the endingBalance value
            }
           
        };
          calculateBalanceBulk = function () {
            const div = document.getElementById("endingBalanceColumn");
            const chargeDifference = parseFloat(self.labelRequest().totalCharge()) - parseFloat(self.labelRequest().totalCost());
            if (isNaN(chargeDifference)) {
                const beginningBalance = parseFloat(self.labelRequest().beginningBalance());
                if (beginningBalance >= parseFloat(self.labelRequest().totalCost())) {
                   
                    div.style.backgroundColor = "rgba(8, 131, 35, 0.66)";
                    isChargedEnough = true;
                    canBuy(true);
                } else {
                    const endingBalance = beginningBalance - parseFloat(self.labelRequest().totalCost());
                    self.labelRequest().endingBalance(endingBalance.toFixed(2));
                    div.style.backgroundColor = "#F17A69";
                    isChargedEnough = false;
                    canBuy(false);
                }
            } else {
                var endingB = parseFloat(self.labelRequest().beginningBalance()) + chargeDifference;
                var roundedEnding = (endingB * 100) / 100;

                if (roundedEnding < 0) {
                    div.style.backgroundColor = "#F17A69";
                    isChargedEnough = false;
                    canBuy(false);
                } else {
                    div.style.backgroundColor = "rgba(8, 131, 35, 0.66)";
                    isChargedEnough = true;
                    canBuy(true);
                }
                self.labelRequest().endingBalance(roundedEnding.toFixed(2)); // Format the endingBalance value
            }
        }


        self.selectUnderRate = function (rate) {
            const og = self.selectedRateUnder70();
            const newRate = rate;
            self.ratesUnder70.replace(newRate,og);
            self.selectedRateUnder70(newRate);
            self.labelRequestUnder70().totalCost(newRate.ourPrice);
            self.labelRequestUnder70().totalCharge(newRate.ourPrice);
            self.createLabelObjectUnder70().selectedClass(newRate.serviceClass);
            calculateBalance();
        };

        self.selectOverRate = function (rate) {
            const og = self.selectedRateOver70();
            const newRate = rate;
            self.ratesOver70.replace(newRate, og);
            self.selectedRateOver70(newRate);
            self.labelRequestOver70().totalCost(newRate.ourPrice);
            self.labelRequestOver70().totalCharge(newRate.ourPrice);
            self.createLabelObjectOver70().selectedClass(newRate.serviceClass);
            calculateBalance();
        };




         var dropzone = document.getElementById('dropzone');

    dropzone.ondragover = function (event) {
        event.preventDefault();
       
    };

   

        dropzone.ondrop = function (event) {
            event.preventDefault();
     

            var file = event.dataTransfer.files[0];
            uploadFile(file);
            };
     

        function uploadFile(file) {
            showLoadingScreen();
            var formData = new FormData();
            formData.append('file', file);

            $.ajax({
                type: 'POST',
                url: '/FileUpload',
                data: formData,
                processData: false,  // tell jQuery not to process the data
                contentType: false,  // tell jQuery not to set contentType
                success: function (data) {
                    hideLoadingScreen();
                    if (data.isError) {
                        alert(data.error)
                    }
                    else {
                        document.getElementById("dropdownrates").style.display = "block";
                        // rates for over and under 70lbs
                        self.ratesOver70(data.over);
                        self.ratesUnder70(data.under);

                        // Selected rates for over and under 70lbs
                        self.selectedRateOver70(data.selectedOver);
                        self.selectedRateUnder70(data.selectedUnder);

                        // Update label objects
                        self.createLabelObjectOver70().selectedClass(self.selectedRateOver70().serviceClass);
                        self.createLabelObjectUnder70().selectedClass(self.selectedRateUnder70().serviceClass);

                        // Set costs
                        self.labelRequestOver70().totalCost(self.selectedRateOver70().ourPrice);
                        self.labelRequestUnder70().totalCost(self.selectedRateUnder70().ourPrice);

                        // Set charges
                        self.labelRequestOver70().totalCharge(self.selectedRateOver70().ourPrice);
                        self.labelRequestUnder70().totalCharge(self.selectedRateUnder70().ourPrice);
                        self.underCount(data.underCount);
                        self.overCount(data.overCount);
                        // Other function calls
                        getUserBalance();
                        calculateBalance();
                        labelID = data.bulkID;
                        self.scrollToRates();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error('File upload failed: ' + errorThrown);
                }
            });
        }





       
        var selectedLabelId = sessionStorage.getItem("selectedLabelId");
        if (selectedLabelId) {
            $.ajax({
                type: 'GET',
                url: "/Dashboard/GetUnfinishedBulk",
                dataType: 'json',
                data: { labelId: selectedLabelId },
                success: function (data) {
                    self.requestForm().LabelId(selectedLabelId);
                    self.requestForm().ToEmail(data.toEmail);
                    self.requestForm().ToPhone(data.toPhone);
                    self.requestForm().ToName(data.toName);
                    self.requestForm().ToCompany(data.toCompany);
                    self.requestForm().ToAddress1(data.toAddress1);
                    self.requestForm().ToAddress2(data.toAddress2);
                    self.requestForm().ToCity(data.toCity);
                    self.requestForm().ToState(data.toState);
                    self.requestForm().ToZip(data.toZip);
                    self.requestForm().FromName(data.fromName);
                    self.requestForm().FromCompany(data.fromCompany);
                    self.requestForm().FromAddress1(data.fromAddress1);
                    self.requestForm().FromAddress2(data.fromAddress2);
                    self.requestForm().FromCity(data.fromCity);
                    self.requestForm().FromState(data.fromState);
                    self.requestForm().FromZip(data.fromZip);
                    self.requestForm().FromPhone(data.fromPhone);
                    self.requestForm().Length(data.length);
                    self.requestForm().Width(data.width);
                    self.requestForm().Height(data.height);
                    self.requestForm().Pounds(data.weight);
                }
            });

            
            // Remove the selectedLabelId from sessionStorage to avoid using it again unintentionally
            sessionStorage.removeItem("selectedLabelId");
        }
        self.beingPurchased = ko.observable(false);
        //rates is a List<RateDTO>
        //self.rates = ko.observableArray();

        ////selectedrate is a RateDTO
        //self.selectedrate = ko.observable();
        //self.createLabelObject = ko.observable({
        //    selectedOrder: ko.observable(),
        //    selectedClass: ko.observable()
        //});





        self.Item = ko.observable({
            selectedOrder: ko.observable(),
            selectedClass: ko.observable()
        });
       
        var isChargedEnough = false;
      

      

     

        self.error = ko.observable();
        $.ajax({
            type: 'GET',
            url: "/Home/getUserDeets/",
            dataType: 'json',
            success: function (data) {
                self.requestForm().FromName(data.fullName);
                self.requestForm().FromPhone(data.phone);

            }
        });

      
        self.scrollToRates = function () {
            var rateContainer = document.getElementById("dropdownrates");
            if (rateContainer) {
                rateContainer.scrollIntoView({ behavior: "smooth", block: "start" });
            }
        };

     

     
      
    
        
        $("#buyLabel").click(function (event) {
            //event.preventDefault();
            // document.getElementById("msgDiv").style.display = "block";
            // change isCharged to hasPayed later
            
          
            if (isChargedEnough) {
              
                self.beingPurchased(true);
                
                // This is your test publishable API key.
                
                 //The items the customer wants to buy
                
               
                function calculateOrderAmount(amount) {
                    const price = parseFloat(amount);

                    if (!isNaN(price)) {
                        // Convert the price to the smallest currency unit (e.g., cents)
                        const smallestCurrencyUnit = Math.round(price * 100);
                        return smallestCurrencyUnit;
                    } else {
                        return 0;
                    }
                }
             
           
                //currently not able to get the Client Secret from fetch below, check errors and openai
               // const stripe = Stripe("pk_test_51MxFCnDHpayIZlcAytKURkjtSmxLNLAd0V2noxps5R1Of0zyHxD67diq4jeehDxzSW2TbyC7Wpu8gDpGi6ros1vU009J6Nf8zm");
                const stripe = Stripe("pk_live_51MxFCnDHpayIZlcAomIsaHnFuJDxnQJxJtQf58k2XzvoK2ZRT5Qwmbam93JzOOcS5HZsuQtZP8dMLU7ac0SsZsz5005fSBdzpr");

                const options = {
                    mode: 'payment',
                    amount: calculateOrderAmount(self.labelRequest().totalCharge()),
                    currency: 'usd',
                    // Fully customizable with appearance API.
                    appearance: {
                        theme: 'night',
                        variables: {
                            colorPrimary: '#827ded',
                            colorBackground: '#171717',
                        },
                    },
                };
                const elements = stripe.elements(options);
                const paymentElement = elements.create('payment');
                paymentElement.mount('#payment-element');


                const form = document.getElementById('payment-form');
                const submitBtn = document.getElementById('submit');

                const handleError = (error) => {
                    const messageContainer = document.querySelector('#error-message');
                    messageContainer.textContent = error.message;
                    submitBtn.disabled = false;
                }

                form.addEventListener('submit', async (event) => {
                    // We don't want to let default form submission happen here,
                    // which would refresh the page.
                    event.preventDefault();

                    // Prevent multiple form submissions
                    if (submitBtn.disabled) {
                        return;
                    }

                    // Disable form submission while loading
                    submitBtn.disabled = true;

                    // Trigger form validation and wallet collection
                    const { error: submitError } = await elements.submit();
                    if (submitError) {
                        handleError(submitError);
                        return;
                    }

                    // Create the PaymentIntent and obtain clientSecret
                    const res = await fetch("/create-intent", {
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify({
                            amount: calculateOrderAmount(self.labelRequest().totalCost()),
                            charged: calculateOrderAmount(self.labelRequest().totalCharge()),
                            description: labelID,
                            serviceClassOver: self.createLabelObjectOver70().selectedClass(),
                            serviceClassUnder: self.createLabelObjectUnder70().selectedClass(),
                            rateDtoOver: self.selectedRateOver70(),
                            rateDtoUnder: self.selectedRateUnder70()
                            
                        })
                    });

                    const { client_secret: clientSecret } = await res.json();

                    // Confirm the PaymentIntent using the details collected by the Payment Element
                    const returnUrl = `${window.location.origin}/Dashboard/PaymentProcessing/`;

                    const { error } = await stripe.confirmPayment({
                        elements,
                        clientSecret,
                        confirmParams: {
                            return_url: returnUrl,
                        },
                    });

                    if (error) {
                        // This point is only reached if there's an immediate error when
                        // confirming the payment. Show the error to your customer (for example, payment details incomplete)
                        handleError(error);
                    } else {
                        // Your customer is redirected to your `return_url`. For some payment
                        // methods like iDEAL, your customer is redirected to an intermediate
                        // site first to authorize the payment, then redirected to the `return_url`.
                    }
                });



            }
        });

      
        

        self.makeTheLabel = function () {
            var root = {
                Rate: self.selectedrate(),
                LabelId: labelID
            }
            $.ajax({
                url: "/Home/makeTheLabel/",
                type: "POST",
                data: root,
                success: function (response) {
                    window.location.href = response.redirectToUrl;
                }
            });
            return false;
        } 
     
    }
    ko.applyBindings(new ViewModel());
   
}
     

if ($("body").data("title") === "Orders") {
   
    var ViewModel = function () {
        showLoadingScreen()
        var self = this;
  
        self.hasNoLabel= ko.observable(false);
        self.hasNotStarted = ko.observable(false);

        self.labels = ko.observableArray();
        self.unfinishedLabels = ko.observableArray();
       
        $.ajax({
            type: 'GET',
            url: "/Dashboard/getLabelDetails/",
            dataType: 'json',
            success: function (data) {
                
                hideLoadingScreen()
                if (data.finishedOrders.length == 0) {
                    self.hasNoLabel(true);
                }
                else {
                    self.labels(data.finishedOrders);
                }
                if (data.unfinishedOrders.length == 0) {
                    self.hasNotStarted(true);
                }

                else {
                    self.unfinishedLabels(data.unfinishedOrders);

                }
            }
        });
       


        self.completeLabel = function (label, event) {
            showLoadingScreen()
            

            // The UnfinishedLabel object is accessible using `label`
            var labelId = label.labelId;

            // Store the selected LabelId in sessionStorage
            sessionStorage.setItem("selectedLabelId", labelId);

            // Redirect the user to the DashboardController/Ship page
            window.location.href = "/Dashboard/Ship";
            hideLoadingScreen()
        };
        self.duplicateLabel = function (label, event) {
            showLoadingScreen()
            

            // The UnfinishedLabel object is accessible using `label`
            var labelId = label.labelId;

            // Store the selected LabelId in sessionStorage
            sessionStorage.setItem("selectedLabelId", labelId);

            // Redirect the user to the DashboardController/Ship page
            window.location.href = "/Dashboard/Ship";
            hideLoadingScreen()
        };


    };

    ko.applyBindings(new ViewModel());

}

if ($("body").data("title") === "Account") {

    var ViewModel = function () {

        var self = this;
        self.userDeets = ko.observable({
            FullName: ko.observable(),
            Email: ko.observable(),
            Phone: ko.observable()
        });
        self.returnAddress = ko.observable({
            FromCompany: ko.observable(),
            FromAddress1: ko.observable(),
            FromAddress2: ko.observable(),
            FromCity: ko.observable(),
            FromState: ko.observable(),
            FromZip: ko.observable(),
            FromPhone: ko.observable(),
           
        });
        $.ajax({
            type: 'GET',
            url: "/Home/getUserDeets/",
            dataType: 'json',
            success: function (data) {
                self.userDeets().FullName(data.fullName);
                self.userDeets().Email(data.email);
                self.userDeets().Phone(data.phone);
                
            }
        });

        $.ajax({
            type: 'GET',
            url: "/Home/getUserAddress/",
            dataType: 'json',
            success: function (data) {
                if (data != undefined) {
                    self.returnAddress().FromAddress1(data.addressLine1);
                    self.returnAddress().FromAddress2(data.addressLine2);
                    self.returnAddress().FromCity(data.city);
                    self.returnAddress().FromState(data.stateProvinceCode);
                    self.returnAddress().FromZip(data.postalCode);
                    self.returnAddress().FromCompany(data.company);
                }

            }
        });
        var accUpdate = false;
        var addressUpdate = false;

        acc = function () {
            accUpdate = true;
        };
        addy = function () {
            showLoadingScreen()
            var request = {
                AddressLine1: self.returnAddress().FromAddress1(),
                AddressLine2: self.returnAddress().FromAddress2(),
                Company: self.returnAddress().FromCompany(),
                City: self.returnAddress().FromCity(),
                StateProvinceCode: self.returnAddress().FromState(),
                PostalCode: self.returnAddress().FromZip(),
                IsReturnAddress: true
            };

            $.ajax({
                type: 'POST',
                url: "/Dashboard/UpdateAddress/",
                data: request,
                success: function (response) {
                    hideLoadingScreen()
                    window.location.reload();
                g}
            });

        };
        
        var isVerified = false;
        self.validatePass = function (formElement) {
            showLoadingScreen()
           
        // Get a reference to the password input field in the popup
            var passwordInput = document.getElementById('passConfirm');

            // Get the password value from the input field
            var password = passwordInput.value;

            // Make an AJAX call to the server to validate the password
            $.ajax({
                type: 'POST',
                url: '/Dashboard/ValidatePassword',
                data: { password: password },
                success: function (data) {
                    // If the password is valid, submit the form
                    hideLoadingScreen()
                    if (data) {
                        if (accUpdate) {
                            var request = {
                                FullName: self.userDeets().FullName(),
                                Email: self.userDeets().Email(),
                                Phone: self.userDeets().Phone()
                            };

                            $.ajax({
                                type: 'POST',
                                url: "/Dashboard/UpdateUserInfo/",
                                data: request,
                                success: function (response) {
                                    window.location.href = response.redirectToUrl;
                                }
                            });

                        }
                       
                       
                    } else {
                        hideLoadingScreen()
                        alert('Invalid password. Please try again.');
                    }
                },
                error: function () {
                    hideLoadingScreen()
                    alert('An error occurred while validating your password. Please try again.');
                }
            });
       
        }
 



    };

    ko.applyBindings(new ViewModel());

}
if ($("body").data("title") === "Admin") {

    var ViewModel = function () {




        ////FOR ORDER OBSERVABLE BELOW, ADD THE REMAINING OBSERVABLES NEEDED TO DISPLAY IN THE ADMIN PAGE AFTER A MF ENTERS LABELID
        /// LIKE ORDER STATUS, ERROR MESSAGE
        /// NEED TO COMBINE LABELDETAILS WITH UPSORDERDETAILS MAKE A NEW DTO AND RETURN 







        var self = this;
        self.order = ko.observable({
            LabelId: ko.observable(),
            UserName: ko.observable(),
            OrderMessage: ko.observable(),
            ErrorMessage: ko.observable(),
            Status: ko.observable(),
            LabelServiceAttempts: ko.observable(),
            FromEmail: ko.observable(),
            FromName: ko.observable(),
            FromCompany: ko.observable(),
            FromPhone: ko.observable(),
            FromAddress1: ko.observable(),
            FromAddress2: ko.observable(),
            FromZip: ko.observable(),
            FromCity: ko.observable(),
            FromState: ko.observable(),
            ToEmail: ko.observable(),
            ToName: ko.observable(),
            ToCompany: ko.observable(),
            ToPhone: ko.observable(),
            ToAddress1: ko.observable(),
            ToAddress2: ko.observable(),
            ToZip: ko.observable(),
            ToCity: ko.observable(),
            ToState: ko.observable(),
            Weight: ko.observable(),
            Length: ko.observable(),
            Width: ko.observable(),
            Height: ko.observable(),
            ServiceClass: ko.observable(),
            OGPrice: ko.observable(),
            PercentSaved: ko.observable(),
            OurPrice: ko.observable(),
            TotalCharge: ko.observable()
           
        });
  

        self.userDeets = ko.observable({
            email: ko.observable(),
            balance: ko.observable()


        });
        
        getOrder = function () {
            showLoadingScreen()
            $.ajax({
                type: 'POST',
                url: "/Dashboard/AdminGetOrderDetails",
                dataType: 'json',
                data: { labelId: self.order().LabelId() },
                success: function (data) {
                    document.getElementById("orderDiv").style.display = "block";
                    hideLoadingScreen()
                    self.order().LabelId(data.labelId);
                    self.order().UserName(data.userName);
                    self.order().OrderMessage(data.orderMessage);
                    self.order().ErrorMessage(data.errorMessage);
                    self.order().Status(data.status);
                    self.order().LabelServiceAttempts(data.labelServiceAttempts);
                    self.order().FromEmail(data.fromEmail);
                    self.order().FromName(data.fromName);
                    self.order().FromCompany(data.fromCompany);
                    self.order().FromAddress1(data.fromAddress1);
                    self.order().FromAddress2(data.fromAddress2);
                    self.order().FromCity(data.fromCity);
                    self.order().FromState(data.fromState);
                    self.order().FromZip(data.fromZip);
                    self.order().FromPhone(data.fromPhone);
                    self.order().ToEmail(data.toEmail);
                    self.order().ToPhone(data.toPhone);
                    self.order().ToName(data.toName);
                    self.order().ToCompany(data.toCompany);
                    self.order().ToAddress1(data.toAddress1);
                    self.order().ToAddress2(data.toAddress2);
                    self.order().ToCity(data.toCity);
                    self.order().ToState(data.toState);
                    self.order().ToZip(data.toZip);
                    self.order().Length(data.length);
                    self.order().Width(data.width);
                    self.order().Height(data.height);
                    self.order().Weight(data.weight);

                    self.order().ServiceClass(data.serviceClass);
                    self.order().OGPrice(data.oGPrice);
                    self.order().PercentSaved(data.percentSaved);
                    self.order().OurPrice(data.ourPrice);
                    self.order().TotalCharge(data.totalCharge);

                }
            });

        };
       

        

       
      

        findEmail = function () {

            showLoadingScreen()
           

            $.ajax({
                type: 'POST',
                url: "/Dashboard/AdminGetUserBalance/",
                data: { email: self.userDeets().email() },
                success: function (response) {
                    hideLoadingScreen()
                    self.userDeets().balance(response.balance);
                    document.getElementById("passwordDiv").style.display = "block";
                    document.getElementById("balanceDiv").style.display = "block";
                }
            });

        };
        
        //var isVerified = false;
        self.validatePass = function (formElement) {
            showLoadingScreen()
           
        // Get a reference to the password input field in the popup
            var passwordInput = document.getElementById('passConfirm');

            // Get the password value from the input field
            var password = passwordInput.value;

            // Make an AJAX call to the server to validate the password
            $.ajax({
                type: 'POST',
                url: '/Dashboard/ValidatePassword',
                data: { password: password },
                success: function (data) {
                    // If the password is valid, submit the form ---------------------------------------------------

                    if (data) {
                        
                            var request = {
                                email: self.userDeets().email(),
                                balance: self.userDeets().balance()
                            };

                            $.ajax({
                                type: 'POST',
                                url: "/Dashboard/AdminUpdateBalance/",
                                data: request,
                                success: function (response) {

                                    hideLoadingScreen()
                                    window.location.href = response.redirectToUrl;
                                }
                            });

                        
                       
                       
                    } else {
                        hideLoadingScreen()
                        alert('Invalid password. Please try again.');
                    }
                },
                error: function () {
                    hideLoadingScreen()
                    alert('An error occurred while validating your password. Please try again.');
                }
            });
       
        }
 



    };

    ko.applyBindings(new ViewModel());

}

