//var unsavedChanges = false;


// Display the whole table
    function DisplayTable(BinLocation)
    {
        $("#table-body tr").show();
    }
    function UpdateTableRow(ID,confirmed,updatedOnHand,updatedLocation,notes)
    {
        $.ajax({
            type: "POST",
            url: "/Home/UpdateDatabaseAjax",
            data: { id: ID, confirmed: confirmed, quantity: updatedOnHand, location: updatedLocation, notes: notes},
            success: function (rowHTML) {
                // Update the row HTML
                $("tr#" + ID).replaceWith(rowHTML);
                var selectedOption = $("select option:selected").val();
                if (selectedOption == "")
                {
                    selectedOption = "All";
                }
                UpdateOption(updatedLocation,selectedOption);
            },
            error: function (result) {
                alert("Something went wrong! The last item changed didn't save!");
            }
        });
    }
    function UpdateOption(updatedLocation,selectedOption)
    {
        // Update select box with another Ajax call
        $.ajax({
            type: "POST",
            url:"/Home/UpdateOptionAjax",
            data:{binLocation:updatedLocation},
            success: function (optionHTML) {
                if (updatedLocation != "")
                {
                    var updatedOption = $("option#" + (updatedLocation.replace(", ", "_")));
                    updatedOption.replaceWith(optionHTML);
                }
                $("option#" + selectedOption.replace(",", "_")).attr('selected', 'selected');
            }
        });
    }
// Ajax call to update database
    function UpdateDatabase(row)
    {
        // Get values from row
        var ID = row.data("id");
        var confirmCheckbox = $("#confirm-check-for-item-id-" + ID);
        var initialOnHand = $("#initial-on-hand-for-item-id-" + ID).text();
        var newOnHand = $("#new-quantity-for-item-id-" + ID).val();
        var currentBinLocation = $("#bin-location-for-item-id-" + ID).text();
        var newBinLocation = $("#new-bin-location-for-item-id-" + ID).val();
        var notes = $("#notes-for-item-id-" + ID).val();

        // Check if the checkbox is checked and set confirmed accordingly
        var confirmed = false;
        if (confirmCheckbox.prop('checked')) {
            confirmed = true;
        }

        // If the new on hand is blank or the item has been confirmed, use the initial 
        var updatedOnHand;
        if (newOnHand == "" || confirmed == true) {
            updatedOnHand = initialOnHand;
        }
        else {
            updatedOnHand = newOnHand
        }

        // If the new bin is blank (if they erased a change), use the current bin location
        var updatedLocation;
        if (newBinLocation == "") {
            updatedLocation = currentBinLocation;
        }
        else {
            updatedLocation = newBinLocation;
        }
        UpdateTableRow(ID, confirmed, updatedOnHand, updatedLocation, notes);

}

// Filter Logic
    function Filter(ItemsCheckbox,ZerosCheckbox,BinLocation)    
    {
        // Select the correct option
        var selectedOption = $("select option:selected").val();
        if (selectedOption == "") {
            selectedOption = "All";
        }
        $("option#" + selectedOption.replace(",","_")).prop('seleted', 'selected');


        // Check for Empty Bin Location
        if (BinLocation == "")
        {
            // Check for both boxes being checked
            if (ItemsCheckbox.prop('checked') && ZerosCheckbox.prop('checked'))
            {
                // Hide the items that have been checked and have no items on hand
                $("#table-body tr[data-counted='true'], [data-zero-on-hand='true']").hide();
                // Show the rest 
                $("#table-body tr[data-counted='false'][data-zero-on-hand='false']").show();

                // Hide the     s for locations that have only zeros and all items counted
                // Filter the filter select list
                $('option').each(function () {
                    var option = $(this);
                    if ((option.attr("data-has-un-counted-non-zeros") == "True")) {
                        // Show the option
                        option.show();
                    }
                    else {
                        option.hide();
                    }
                });
            }

            // Check for items checkbox checked
            else if (ItemsCheckbox.prop('checked') && ZerosCheckbox.prop('checked') == false)
            {
                // Display ALL Items that have not been counted

                // Hide the items checked
                $("#table-body tr[data-counted='true']").hide();

                // Show the rest
                $("#table-body tr[data-counted='false']").show();

                // Hide Bin Locations that have all of their items counted
                $('option').each(function () {
                    var option = $(this);
                    if ((option.attr("data-has-only-non-zeros") == "False")) {
                        // Hide the option
                        option.show();
                    }
                    if ((option.attr("data-all-items-counted") == "True") && (option.attr("data-has-only-non-zeros") == "True")) {
                        // Hide the option
                        option.hide();
                    }
                });
            }

            // Check for zeros checkbox checked
            else if (ItemsCheckbox.prop('checked')==false && ZerosCheckbox.prop('checked'))
            {
                // Display ALL Items that do not have zeros
                // Hide the zeros
                $("#table-body tr[data-zero-on-hand='true']").hide();

                // Show the rest
                $("#table-body tr[data-zero-on-hand='false']").show();

                // Hide Bin Locations that have only zeros
                // Filter the filter select list
                $('option').each(function () {
                    var option = $(this);
                    if ((option.attr("data-has-un-counted-non-zeros") == "False")) {
                        // Show the option
                        option.show();
                    }
                    if ((option.attr("data-has-only-non-zeros") == "False")) {
                        // Hide the option
                        option.hide();
                    }
                });
            }

            
            // Display the whole table (Nothing checked)
            else
            {
                DisplayTable();
                // Display all of the options
                $('option').show();
            }
        }
        else // Same functionality, except including Bin Location
        {
            // Check for both boxes being checked
            if (ItemsCheckbox.prop('checked') && ZerosCheckbox.prop('checked')) {

                // Hide the items that have been checked and have no items on hand
                $("#table-body tr[data-bin-location!='" + BinLocation + "'],[data-zero-on-hand='true'],[data-counted='true']").hide();
                // Show the rest items that have been chacked and have items on hand
                $("#table-body tr[data-zero-on-hand='false'][data-bin-location='" + BinLocation + "'][data-counted='false']").show();


                // Hide the options for locations that have only zeros and all items counted
                // Filter the filter select list
                $('option').each(function () {
                    var option = $(this);
                    if ((option.attr("data-has-un-counted-non-zeros") == "True")) {
                        // Show the option
                        option.show();
                    }
                    else {
                        option.hide();
                    }
                });
            }

            // Check for items checkbox checked
            else if (ItemsCheckbox.prop('checked') && ZerosCheckbox.prop('checked') == false) {
                // Display the Items that have not been counted

                // Hide the items not in the bin
                $("#table-body tr[data-bin-location!='" + BinLocation + "'],[data-counted='true']").hide();
                $("#table-body tr[data-bin-location='" + BinLocation + "'][data-counted='false']").show();

                // Hide Bin Locations that have all of their items counted
                $('option').each(function () {
                    var option = $(this);
                    if ((option.attr("data-has-only-non-zeros") == "False")) {
                        // Hide the option
                        option.show();
                    }
                    if ((option.attr("data-all-items-counted") == "True") && (option.attr("data-has-only-non-zeros") == "True")) {
                        // Hide the option
                        option.hide();
                    }
                });

            }

            // Check for zeros checkbox checked
            else if (ItemsCheckbox.prop('checked') == false && ZerosCheckbox.prop('checked')) {
                // Display ALL Items that do not have zeros

                // Hide the items not in the bin
                $("#table-body tr[data-bin-location!='" + BinLocation + "'],[data-zero-on-hand='true']").hide();
                $("#table-body tr[data-bin-location='" + BinLocation + "'][data-zero-on-hand='false']").show();

                // Hide Bin Locations that have only zeros
                // Filter the filter select list
                $('option').each(function () {
                    var option = $(this);
                    if ((option.attr("data-all-items-counted") == "True")||(option.attr("data-has-un-counted-non-zeros")=="False")) {
                        // Hide the option
                        option.show();
                    }
                    if ((option.attr("data-has-only-non-zeros") == "False")) {
                        // Hide the option
                        option.hide();
                    }
                });

            } 


            // Display the whole table (Nothing checked)
            else{

                // Hide everything that doesn't have the bin location
                $("#table-body tr[data-bin-location!='" + BinLocation + "']").hide();
                $("#table-body tr[data-bin-location='" + BinLocation + "']").show();

                // Display all of the options
                $('option').show();

            }
        }
        // Always show the All option and hide the blank option
        $('option').each(function () {
            var option = $(this);
            if(option.text()=="All")
            {
                option.show();
            }
            else if(option.text()=="")
            {
                option.hide();
            }

        });
    }
        
    $(document).ready(function () {

        $(".fancybox").fancybox({
            type: "ajax",
            width: '90%',
            height: '90%',
            autoSize: false,
            beforeClose: function () {
                unsavedChanges = true;

                // Get values
                var ID = $("#description-fancybox").data("fancybox-id");
                var notesTextValue = $(".fancybox-text-area").val();
                var newBinTextBox = $(".fancybox-text-input").val();

                // Set values
                var notes = $("#notes-for-item-id-" + ID);
                notes.val(notesTextValue);
                var binLocation = $("#new-bin-location-for-item-id-" + ID);
                binLocation.val(newBinTextBox);

                // Select the Description box and change color if changes were made
                var descriptionTextBox = $("#description-for-item-id-" + ID);

                if ((notesTextValue == "") && (newBinTextBox == "")) {
                    linkButton.attr("data-empty", "true") // Both inputs are empty
                }
                else {
                    linkButton.attr("data-empty", "false");// An input has something stored
                }
                var row = notes.closest("tr");
                UpdateDatabase(row);

            }
        });
        
        // Whenever someone post, show the loading button
        $(".post-button").click(function () {
            var image = $(".loading-in-progress");
            image.show();
        });

        // Updating whether or not the item has been Confirmed
        $("#table-body").on("change", ":checkbox", function () {
            // Gather variables
            var checkbox = $(this);

            // Item has been confirmed
            if (checkbox.attr("id") != "get-rid-of-zero" && checkbox.attr("id") != "items-not-counted") {
                var textbox = checkbox.closest("tr").find(".input-checker");
                var row = checkbox.closest("tr");
                var textBoxInput = textbox.find(":input");
                var initialOnHand = row.find(".initial-on-hand").attr("data-initial-on-hand");
                var newOnHand = row.find(".initial-on-hand").attr("data-new-on-hand");




                // Handles changing row color
                // NaN is the value returned if there is nothing in the textbox
                if (newOnHand == initialOnHand || newOnHand == "NaN") {
                    row.attr("data-quantity-changed", "false");
                }
                else {
                    row.attr("data-quantity-changed", "true");
                }

                if (checkbox.prop('checked')) {
                    // Update Summary numbers if it doesn't originally have 0 on hand
                    if (initialOnHand != 0) {
                        var ItemsCounted = parseInt($("#items-counted").text()) + 1;
                        var ItemsLeft = parseInt($("#items-left").text()) - 1;
                        $("#items-counted").text(ItemsCounted);
                        $("#items-left").text(ItemsLeft);
                    }

                    row.attr("data-confirm-checked", "true");
                    row.attr("data-counted", "true");
                }
                else { //  Unchecked 

                    // Update Summary numbers if it doesn't originally have 0 on hand
                    if (initialOnHand != 0) {
                        var ItemsCounted = parseInt($("#items-counted").text()) - 1;
                        var ItemsLeft = parseInt($("#items-left").text()) + 1;
                        $("#items-counted").text(ItemsCounted);
                        $("#items-left").text(ItemsLeft);
                    }

                    row.attr("data-confirm-checked", "false");
                    row.attr("data-counted", "false");
                }
                textbox.toggle();
            }
        });   
    
        // Updating whether or not they have entered a new quantity
    $("input[type='number']").change(function () {
        var textbox = $(this);
        var row = textbox.closest("tr");
        var newQuantity = parseFloat(textbox.val());
        var newOnHand = row.find(".initial-on-hand").attr("data-new-on-hand", newQuantity);
        var initialOnHand = row.find(".initial-on-hand").attr("data-initial-on-hand");


        if(newQuantity.toString()=="NaN" || newQuantity.toString() == null)
        {
            row.attr("data-quantity-changed", "false");
            // Update Summary numbers
            if (initialOnHand != 0)
            {
                var ItemsCounted = parseInt($("#items-counted").text()) - 1;
                var ItemsLeft = parseInt($("#items-left").text()) + 1;
            }
            $("#items-counted").text(ItemsCounted);
            $("#items-left").text(ItemsLeft);
            row.attr("data-counted", "false");
        }
        else
        {
            if (initialOnHand != 0)
            {
                var ItemsCounted = parseInt($("#items-counted").text()) + 1;
                var ItemsLeft = parseInt($("#items-left").text()) - 1;
            }
            $("#items-counted").text(ItemsCounted);
            $("#items-left").text(ItemsLeft);
            row.attr("data-quantity-changed", "true");
            row.attr("data-counted", "true");
        }

    });

        // BinLocation Change
    $("#bin-location").change(function () {
        var BinLocation = $(this).val();
        var ZerosCheckBox = $("#get-rid-of-zero");
        var ItemsCheckBox = $("#items-not-counted");
        Filter(ItemsCheckBox, ZerosCheckBox, BinLocation);
    });

        // Zero Checkbox Change
    $("#get-rid-of-zero").change(function () {
        var ZerosCheckbox = $(this);
        var BinLocation = $("#bin-location").val();
        var ItemsCheckBox = $("#items-not-counted");
        Filter(ItemsCheckBox, ZerosCheckbox, BinLocation);
    });

        // Items Uncounted Change
    $("#items-not-counted").change(function(){
        var ItemsCheckBox = $(this);
        var BinLocation = $("#bin-location").val();
        var ZerosCheckBox = $("#get-rid-of-zero");
        Filter(ItemsCheckBox,ZerosCheckBox,BinLocation);
    });


    $("#table-body").on("change",".ajax-input", function () {
        // Update database
        var input = $(this);
        var row = input.closest("tr");
        UpdateDatabase(row);
    });

    // Apply Filter after a post
    if ($("#items-not-counted").prop('checked') || $("#get-rid-of-zero").prop('checked') || $("#bin-location").val() != "") {
        var ItemsCheckBox = $("#items-not-counted");
        var BinLocation = $("#bin-location").val();
        var ZerosCheckBox = $("#get-rid-of-zero");
        Filter(ItemsCheckBox, ZerosCheckBox, BinLocation);
    }


});

$(window).load(function () {
    $("#loading-in-progress").hide();
});