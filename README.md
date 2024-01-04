<p align="center">
  <img src="https://raw.githubusercontent.com/belowaverage-org/Tikhole/master/Tikhole.Website/wwwroot/logo.svg" />
</p>
<hr />

Welcome to Tikhole, your DNS forwarder with the power of black hole filtering for MikroTik routers. Unleash the cosmic force of DNS-based firewall filtering and take control of your network's destiny.

## Overview

Tikhole is a forward-thinking DNS forwarder that goes beyond traditional methods. By connecting to a MikroTik router, Tikhole dynamically updates address-lists based on DNS responses, allowing you to implement precise IP-based firewall filtering.

## Features 🚀

- **Dynamic DNS Filtering**: React to DNS responses in real-time and apply filtering rules to MikroTik router address-lists.

- **Regex Rule Matching**: Define rules on the rules page using regex patterns to selectively filter DNS responses.

- **MikroTik Integration**: Seamlessly connect Tikhole to your MikroTik router, enhancing your network's security with IP-based filtering.

- **Real-time Web Interface**: Configure Tikhole, view real-time logs, and monitor DNS requests passing through Tikhole, all through a user-friendly web interface.

- **Cosmic Control**: Take charge of your network's destiny by gaining control over IP addresses linked to DNS queries.

## Getting Started with Docker 🚢

### Prerequisites

Before you embark on your cosmic journey with Tikhole, ensure you have the following installed on your system:

- [Docker](https://www.docker.com/get-started)

### Launch Tikhole with Docker

1. **Pull the Tikhole Docker Image:**
    ```bash
    docker pull ghcr.io/belowaverage-org/tikhole:latest
    ```

2. **Launch Tikhole with Docker:**
    ```bash
    docker run -d -p 8080:80 -v /path/to/config/folder:/Tikhole.Website/config --name Tikhole ghcr.io/belowaverage-org/tikhole:latest
    ```

3. **Explore DNS Filtering:**
    Open your web browser and witness the magic at [http://localhost:8080](http://localhost:8080).
