# Embrace Internal Docs
These are the internal docs for working on the Embrace SDK. The structure of this repository is that this is the test repository for the open source Embrace SDK.

## Structure
This project will not have in it the code that ships with the Embrace SDK. Instead, you will need to pull down the open source SDK separately.

## Workflows
To handle workflows properly between both the open source and the private internal repositories, we use the Repository Dispatch strategy. Essentially, recipient repositories implement the repository dispatch signal receiver, an example of which can be found on any of the dispatch trigger files. In the open source SDK, workflows then construct a REST request that targets that signal receiver's specific repository dispatch signal. Any of the workflows in the open source SDK will have an example of this.

## Workflow Key
Important to note: the above strategy requires a Bearer Authorization token that is stored in the Github configuration of the open source SDK. That expires once each year and will need to be renewed in: February (as of 2024).

## Submodules
This repository has a submodule dependency on the public SDK. As a result, you will need to run `git submodule update --init --recursive` on the root folder before you're fully able to work in the repository.
Refer to https://git-scm.com/book/en/v2/Git-Tools-Submodules for more information for working with submodules.