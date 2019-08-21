# PureSocketCluster
**A Cross Platform SocketCluster Client for .NET Core NetStandard**

**[NuGet Package](https://www.nuget.org/packages/PureSocketCluster)** [![PureSocketCluster](https://img.shields.io/nuget/v/PureSocketCluster.svg)](https://www.nuget.org/packages/PureSocketCluster/) 

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/190a8fd38ad84346acfc13022e9b4e56)](https://www.codacy.com/app/ByronAP/PureSocketCluster?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Coinigy/PureSocketCluster&amp;utm_campaign=Badge_Grade)

##### Requirements
* .NET NetStandard V2.0+

##### Usage
* Example Included in project
  
  Provided by: 2018 - 2019 Coinigy Inc. Coinigy.com

## V4 Breaking Changes
* Events now have a sender object which is the instance that raised the event.
* (non breaking but useful) An addition construstor has been added with an instance name argument which makes it easier to identify an instance when using multiple connections. This links to the new property InstanceName.
