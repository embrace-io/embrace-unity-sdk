#!/bin/bash
set -o errexit -o nounset -o pipefail -o xtrace

# This script was derived from the Dockerfiles in the game-ci/docker repository.
# See: https://github.com/game-ci/docker/tree/main/images/ubuntu

# Set frontend to Noninteractive in Debian configuration.
# https://github.com/phusion/baseimage-docker/issues/58#issuecomment-47995343
echo 'debconf debconf/frontend select Noninteractive' | sudo debconf-set-selections

# Global dependencies
sudo apt-get update
sudo apt-get install -y --no-install-recommends apt-utils
sudo apt-get install -y --no-install-recommends --allow-downgrades \
    atop \
    ca-certificates \
    cpio \
    curl \
    git \
    git-lfs \
    gnupg \
    jq \
    libasound2 \
    libc6-dev \
    libcap2 \
    libgbm1 \
    libgconf-2-4 \
    libglu1 \
    libgtk-3-0 \
    libncurses5 \
    libnotify4 \
    libnss3 \
    libstdc++6 \
    libxss1 \
    libxtst6 \
    lsb-release \
    openssh-client \
    unzip \
    wget \
    xvfb \
    xz-utils \
    zenity \
    zip
sudo git lfs install --system --skip-repo

# Fix "No useable version of libssl" for Ubuntu 22.04
# https://forum.unity.com/threads/workaround-for-libssl-issue-on-ubuntu-22-04.1271405/
wget http://security.ubuntu.com/ubuntu/pool/main/o/openssl/libssl1.1_1.1.1f-1ubuntu2_amd64.deb
sudo dpkg -i libssl1.1_1.1.1f-1ubuntu2_amd64.deb
rm libssl1.1_1.1.1f-1ubuntu2_amd64.deb

# Disable default sound card, which removes ALSA warnings
printf 'pcm.!default {\n  type plug\n  slave.pcm "null"\n}\n' | sudo tee /etc/asound.conf >/dev/null

# Bug in xvfb-run that causes it to redirect stderr to stdout. We want it separate
sudo sed -i 's/^\(.*DISPLAY=:.*XAUTHORITY=.*\)\( "\$@" \)2>&1$/\1\2/' /usr/bin/xvfb-run

# Support forward compatibility for Unity activation
echo "576562626572264761624c65526f7578" | sudo tee /etc/machine-id >/dev/null
sudo mkdir -p /var/lib/dbus/
sudo ln -sf /etc/machine-id /var/lib/dbus/machine-id

# Install Unity Hub
# https://docs.unity3d.com/hub/manual/InstallHub.html#install-hub-linux
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg >/dev/null
echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" | sudo tee /etc/apt/sources.list.d/unityhub.list >/dev/null
sudo apt-get update
sudo apt-get install -y unityhub

# Alias to "unity-hub" with default params
printf '#!/bin/bash\nxvfb-run --auto-servernum /opt/unityhub/unityhub-bin --no-sandbox "$@"\n' | sudo tee /usr/bin/unity-hub >/dev/null
sudo chmod +x /usr/bin/unity-hub

# Configure
mkdir -p "/opt/unity/editors"
unity-hub --headless install-path --set "/opt/unity/editors/"
