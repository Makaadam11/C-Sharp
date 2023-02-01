This is command-line interface (CLI) application written in C# 
which will download a file (.zip, .csv, .json)  from a given URL using HTTP, 
parse it and write to standard output result of one of two commands: count or max-age.

Count command
interns.exe count <url> [ --age-gt | --age-lt age] This command counts number of interns satisifing condition and writes it to standard output. Options:

--age-gt <age> - counts interns where age is greater than <age>, where <age> is an integer
--age-lt <age> - counts interns where age is less than <age>, where <age> is an integer
For instance: interns.exe count https://fortedigital.github.io/Back-End-Internship-Task/interns.json should write to standard output: 5

interns.exe count https://fortedigital.github.io/Back-End-Internship-Task/interns.json --age-gt 22 should write to standard output: 2

Max age command
interns.exe max-age <url>

This command writes a maximum age of an intern to standard output.

For instance: interns.exe max-age https://fortedigital.github.io/Back-End-Internship-Task/interns.json should write to standard output: 24

Examples of files are accessible from the following URLs:

https://fortedigital.github.io/Back-End-Internship-Task/interns.json
https://fortedigital.github.io/Back-End-Internship-Task/interns.csv
https://fortedigital.github.io/Back-End-Internship-Task/interns.zip
