﻿if ($("body").data("title") === "Rates") {

    var ViewModel = function () {

        var self = this;
        //defines an observableArray named rates, kina like var rates = new List<Rates>()
        self.rates = ko.observableArray();
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






        self.error = ko.observable();

        self.sendRequest = function (formElement) {
            //document.getElementById("msgDiv").style.display = "block";
            //creates a request object using the values from self.requestForm

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
                    //sends it to the rates variable which is a List<Rates> which have
                    //the values data-binded to div block values in the html 
                    self.rates(data)
                }
            });
        }
    };

    ko.applyBindings(new ViewModel());
}


if ($("body").data("title") === "CreateLabel") {
   
    var ViewModel = function () {

        var self = this;
        var now = new Date();
        var date = now.toLocaleDateString();
        self.rates = ko.observableArray();
        self.selectedrate = ko.observable();
        self.createLabelObject = ko.observable({
            selectedOrder: ko.observable(),
            selectedClass: ko.observable()
        });
        self.labelRequest = ko.observable(
            {

                beginningBalance: ko.observable(100),
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
        calculateBalance = function() {

           const chargeDifference = parseFloat(self.labelRequest().totalCharge()) - parseFloat(self.labelRequest().totalCost());
           var endingB = (parseFloat(self.labelRequest().beginningBalance()) + chargeDifference);
           var roundedEnding = (endingB * 100) / 100;
            const div = document.getElementById("endingBalanceColumn");
           // if (isNan(roundedEnding)
           //{roundedEnding = self.labelRe }
            if (roundedEnding < 0) {
               //if negative

                div.style.backgroundColor = "#F17A69";
                isChargedEnough = false;
           } else {
                div.style.backgroundColor = "rgba(8, 131, 35, 0.66)";
                isChargedEnough = true;
           }

           self.labelRequest().endingBalance(Math.round(roundedEnding));
        }
        //defines a requestForm Object w observables placed in inputs to retrieve the value
        self.requestForm = ko.observable({
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
            IsReturnAddress: ko.observable(),
            Length: ko.observable(),
            Width: ko.observable(),
            Height: ko.observable(),
            Pounds: ko.observable(),
            Ounces: ko.observable(),
            Weight: ko.observable(),
            zeroValueMessage: ko.observable("")
        });
        var madeWeight = false;
        isOverweight=function () {

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
        
        function addressAutocomplete(containerElement, callback, options) {

            const MIN_ADDRESS_LENGTH = 6;
            const DEBOUNCE_DELAY = 300;

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
            console.log("Selected option: ");
            console.log(data);
        }, {
            placeholder: "Enter an address here"
        });

        self.sendRequest = function (formElement) {
            //document.getElementById("msgDiv").style.display = "block";
            if (madeWeight) {
                var after2 = self.requestForm().Weight();
                var request = {
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
                console.log(request);
                $.ajax({
                    type: 'POST',
                    url: "/Home/GetFullRates/",
                    dataType: 'json',
                    data: request,
                    success: function (data) {
                        document.getElementById("dropdownrates").style.display = "block";
                        self.selectedrate(data.selectedrate)
                        self.createLabelObject().selectedClass(data.selectedrate.serviceClass)
                        self.rates(data.rates)
                        self.labelRequest().totalCost(data.selectedrate.ourPrice);
                        self.labelRequest().totalCharge(data.selectedrate.ourPrice);
                        calculateBalance()
                    }
                });
            } else if (!madeWeight) {
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
    

        $("#createLabelButton").click(function (event) {
            event.preventDefault();
            //document.getElementById("msgDiv").style.display = "block";
            //change isCharged to hasPayed later
            if (isChargedEnough) {
                var after2 = self.requestForm().Weight();
                var labelRequest = {
                    order: self.createLabelObject().selectedOrder(),
                    serviceClass: self.createLabelObject().selectedClass()
                };
                var root = labelRequest;

                $.ajax({
                    url: "/Home/makeTheLabel/",
                    type: "POST",
                    data: root,
                    success: function (response) {
                        window.location.href = response.redirectToUrl;
                    }
                });
                return false;
            } else if (!isChargedEnough) {
                alert("The Total Charge amount is not enough for this purchase.")
            }
        });
       

    };

    ko.applyBindings(new ViewModel());

}

if ($("body").data("title") === "Orders") {
   
    var ViewModel = function () {

        var self = this;
  
        self.hasNoLabel= ko.observable(false);

        self.labels = ko.observableArray();

        $.ajax({
            type: 'GET',
            url: "/Home/getLabelDetails/",
            dataType: 'json',
            success: function (data) {
                self.labels(data);
                if (data.length == 0) {
                    self.hasNoLabel(true);
                }
            }
        });

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
            addressUpdate = true;
        };
        
        var isVerified = false;
        self.validatePass = function (formElement) {
           
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
                        if (addressUpdate)
                        {
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
                                    window.location.href = response.redirectToUrl;
                                }
                            });


                        }
                       
                    } else {
                        alert('Invalid password. Please try again.');
                    }
                },
                error: function () {
                    alert('An error occurred while validating your password. Please try again.');
                }
            });
       
        }
 



    };

    ko.applyBindings(new ViewModel());

}
