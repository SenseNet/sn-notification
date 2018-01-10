# Notification for sensenet ECM
Notification component for the [sensenet ECM](https://github.com/SenseNet/sensenet) platform. Lets users subscribe to content changes and receive **emails** either almost immediately or in an aggregated way periodically about changes in the repository.

[![Join the chat at https://gitter.im/SenseNet/sn-notification](https://badges.gitter.im/SenseNet/sn-notification.svg)](https://gitter.im/SenseNet/sn-notification?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![NuGet](https://img.shields.io/nuget/v/SenseNet.Notification.Install.svg)](https://www.nuget.org/packages/SenseNet.Notification.Install)

You may install this component even if you only have the **sensenet ECM Services** main component installed. That way you'll get the backend notification framework with a .Net API. 

> Currently there is no REST api for managing notifications, but you can create custom actions for that in your project - or better yet, contribute them to the official component!

If you also have the [sensenet ECM WebPages](https://github.com/SenseNet/sn-webpages) component installed (which gives you a UI framework built on *ASP.NET WebForms* for sensenet ECM), you'll also get UI elements: notification actions and portlets with content views - a full user interface for subscribing to content events and an admin UI for managing notifications.

> To find out which packages you need to install, take a look at the available [sensenet ECM components](http://community.sensenet.com/docs/sensenet-components).

> To learn more about notifications in sensenet ECM, visit the [main Notification article](/docs/notification.md).

## Installation
To get started, install the Notification component from NuGet:
- [Install sensenet ECM Notification from NuGet](/docs/install-notification-from-nuget)
