# GPT Spam Filter / Mail Sorter
Yep, very original idea about using AI to organize my mail inbox, but Microsoft's spam filter is terrible and lets anything through these days.
Since the project involves sensitive user credentials I won't offer a pre-built version, pull the project and build it your self (good excuse to not set-up CI/CD).

## About
Uses ChatGPT to categorize the inbox of your mail by supplying the mails sender:subject to GPT and having it identify the best suitable category.
Uses Chat GPT 3.5 over Prompt Davinci for cost efficiencies and it gave better results when identifying emails.
Processes about 500 e-mails per $0.002


GPT 3.5 identifies approximately 80% of the mails correctly and tends to lean more towards throwing everything into Work folder, I'm getting better results with GPT 4, but it is not yet openly available.

## Setup
1. Copy appsettings.json to appsettings.production.json
2. Fill in the required fields.
3. Make sure your configured categories exist under your Inbox.
4. Compile and run.

## Roadmap
- [x] Expand on appsettings.json.
   - [x] Configure categories.
   - [x] Configure GPT models.
- [ ] Unit testing
   - [ ] Test GptService
   - [ ] Test MailService