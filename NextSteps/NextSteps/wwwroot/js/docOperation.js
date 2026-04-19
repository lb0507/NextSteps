
//    Document Library Page
//    4/19/ 2026
//    ======================================
//    - Initial page creation
//    ======================================
//    Javascript functions for document related operations (namely downloading)


// Downloads the passed in document
function downloadDocument(fileName, base64)
{
    // Create an 'a' link HTML element
    var link = document.createElement('a');
    // Set the element to download the passed in docuement
    link.download = fileName;
    link.href = "data:application/octet-stream;base64," + base64;
    // Simulate a click on the link to initiate the download
    link.click();
}