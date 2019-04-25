# Global Azure Bootcamp:</br> Generating PDFs at Scale

The aim of this repository is to provide an example solution for generating PDFs in the cloud.

It was presented as part of a presentation at Global Azure Bootcamp 2019.

---

## Navigating this Repository 

This repository has two main directories:
- API
- StressTest

### API

This contains a set of Azure functions that are used a set of endpoints to be called. It contains two functions:

- Generate: Generate a PDF
- RequestStatus: Check the current status of a PDF's generation by PDF ID.

### StressTest

This contains a simple console application which can be used to stress test the Azure functions once they have been deployed.

---