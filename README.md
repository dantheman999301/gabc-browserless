# Global Azure Bootcamp:</br> Generating PDFs at Scale

The aim of this repository is to provide an example solution for generating PDFs in the cloud.

It was presented as part of a presentation at Global Azure Bootcamp 2019.

---

## Navigating this Repository

This repository has two main directories:

- API
- StressTest

### API

This contains a set of Azure functions that are used a set of endpoints to be called. It contains three functions:

- Generate: Generate a PDF
- RequestStatus: Check the current status of a PDFs generation by PDF ID.
- GenerateAndSave: Communicates with Browserless to generate the PDFs and saves them to Blob Storage

### StressTest

This contains a simple console application which can be used to stress test the Azure functions once they have been deployed.

---

## Deploying to Azure

The project currently uses Terraform to deploy all the infrastructure necessary in Azure to run the API project and to run Browserless.

If you wish to use it, we would first suggest that you quickly run through the getting started guide located here:
https://learn.hashicorp.com/terraform/?track=azure#azure

You only need to go a through topics in.

Once you're familiar with roughly how it works, run `az login` to login to Azure and then run `terraform apply`. This will deploy it to your currently selected Azure subscription.

After that, load up the solution in Visual Studio and deploy `Browserless.API` through the publish menu.

All the necessary settings should be set up out the box!

One note of caution! The S2 box it deploys can be quite expensive even with free Azure credits so after you're done playing run `terraform destroy` to delete all the resources.

### Running Locally

If you wish to just run it locally, you'll still need to deploy most of the resources, then update `local.settings.json` with the app settings in `browserless.tf` file.

## Questions!

If you have any questions or feedback, please raise and issue!
