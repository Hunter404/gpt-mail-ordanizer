# GPT Spam Filter / Mail Sorter
Yep, very original idea about using AI to organize my mail inbox, but Microsoft's spam filter is terrible and lets anything through these days.
Since the project involves sensitive user credentials I won't offer a pre-built version, pull the project and build it your self (good excuse to not set-up CI/CD).

## Setup
1. Copy appsettings.json to appsettings.production.json
2. Fill in the required fields.
3. Make sure these folders exist under your Inbox or edit MainService.cs:91
    * Personal
    * Work
    * Spam
    * Newsletters
    * Social
    * Purchases
    * Other
4. Compile and run.

## Roadmap
I don't know where I will go with this honestly, come with suggestions.
