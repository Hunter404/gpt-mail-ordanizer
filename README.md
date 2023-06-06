# GPT Spam Filter / Mail Sorter
Yep, very original idea about using AI to organize my mail inbox, but Microsoft's spam filter is terrible and lets anything through these days.
Since the project involves sensitive user credentials I won't offer a pre-built version, pull the project and build it your self (good excuse to not set-up CI/CD).

## About
Uses ChatGPT to categorize the inbox of your mail by supplying the mails sender:subject to GPT and having it identify the best suitable category.
Uses Chat GPT 3.5 over Prompt Davinci for cost efficiencies and it gave better results when identifying emails.
Processes about 500 e-mails  

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
- [ ] Expand on appsettings.json.
   - [ ] Configure categories.
   - [ ] Configure GPT models.
