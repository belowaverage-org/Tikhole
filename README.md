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

4. **Configure Tikhole through the Web Interface:**
    - Login to the web interface at [http://localhost:8080](http://localhost:8080).
    - Use your MikroTik router login credentials since Tikhole validates them over the MikroTik API.
    - Ensure your MikroTik router has the API enabled.
    - If your MikroTik router is configured with the default IP of `192.168.200.1` and the default API port, no configuration change will be nessesary. Otherwise, edit the Tikhole config file manually at `/Tikhole.Website/config/Tikhole.xml`.
  
5. **Editing Tikhole Configuration Manually:**
    - After running Tikhole once, the configuration file (`Tikhole.xml`) will auto-generate.
    - Edit the file with your specific MikroTik router details and configurations.
    - Restart Tikhole for the changes to take effect.

6. **Set MikroTik API IP, Port, Username, and Password:**
    - Browse to the Settings page on the web interface.
    - Configure the MikroTik router API ip, port, username, and password.
    - It is advisable to create a dedicated user on the router with full API permissions enabled for security.
