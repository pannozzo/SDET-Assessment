2024-08-04

I wrote this from the perspective that I'm testing an API/website that's
being given to me to find places for improvement. There are multiple
tests here that will fail, this is deliberate and based on my 
interpretation of how the API/website should function from the prompt. 
The text below explains why some of these tests fail & my rational for them.

## API Tests

Under API Tests\APITestsFailureCases, I tested invalid inputs for the 
temperature & summary, they returned 201's when I believed they should've
been Bad Requests. I did not test invalid date inputs, as this would 
just be testing Microsoft's DateOnly data structure & not the API.

In POST_API_Should_Rate_Limit_After_Rapid_Requests() I checked whether the
API would rate limit when spammed with a lot of requests, it sends 10000 POST
requests within ~6 seconds, and returns a Created status code & an Id every time.
Expected behavior would be some kind of failure status code preventing an individual
from overwhelming the API & database with junk data. This test fails accordingly.

Under API Tests\APITestsSuccessCases, I tested that the conversion from
the input temperature in Celsius should have correctly converted to its
Fahrenheit equal in `POST_Temperature_In_Celsius_Should_GET_Correct_Fahrenheit()``.
This was not the case. While rounding errors can happen and should be addressed,
this conversion was not correct for the integer conversions. -40C should equal -40F,
100C should equal 212F, etc.

The Fahrenheit conversion should not overflow for high values of TemperatureC
Expected behavior would be an implementation detail, this could be also setting 
TemperatureF to the max integer value, an optional error message in the response body, 
etc. But returning an integer overflow is definitely not good output for the user,
and this test fails because of that.

## End To End Integration Test

In API Tests/EndToEndTest.cs, I wrote a test that travelled the whole path 
from POSTing a weather forecast object to the API, GETing it back, then taking
that object and uploading it to the website's "Weather" page, and then downloading
it back. Finally, comparing the uploaded object to the downloaded object and
seeing if data was preserved. 

First thing I found was that the number of key/value pairs wasn't equal, the website
stripped away the "Id" that we get from GET /weatherforecast/{id}. This is definitely
its own problem, while the website could opt to not display the ID, if it's offering to
relay the data back to the user it ought to preserve all of it. 

In a professional setting I'd have a test for both this AND checking the contents of the 
JSON itself, but for brevity of reviewing this code test I opted to handle this within
the test by removing the ID from the input right before the comparison at the end of the
test. 

However, the comparison still fails, the website is converting the first capital letter in
TemperatureC & TemperatureF to lowercase. JSON is a case sensitive grammar, so this causes 
the objects to not be equal to each other. I am leaving this as a failed test because I believe
this is a bug, if the user wanted to take this download and apply it back to the API, they would
have to manipulate the file to change the name of the key which I feel is a negative user experience.

## Ignored Tests

I have 4 ignored tests, all of them related to downloading files. 
Two of them are in the End_To_End_Integration_Test, and two of them are in the 
Downloaded_Data_Reflects_Uploaded_Data_On_The_Page Weather test. 
I am testing Chrome, Firefox, and Edge for every website test. These three browsers require
different options set to configure where the downloaded file is located. Chrome was moderately
straightforward, FireFox/Edge less so. Given the limited time and that I feel I've demonstrated ability
to test downloaded files in Chrome & test cross-browser in the rest of the project, I felt it was 
the right decision to ignore these 4 test cases, and focus on completing the rest of the code test.

## ETC

Chrome Website Tests/Weather Tests/`Downloaded_Data_Reflects_Uploaded_Data_On_The_Page()` is 
a little flaky, specifically when running the whole test suite. I can run it 30 times consecutively
without a failure, but sometimes when running the whole suite the test fails. 