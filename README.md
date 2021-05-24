> This project is still in the early stages of development. All code, interfaces and features are unstable.

<p align="center">
<img src="./banner.png" width="800px"/>
</p>

Digital asset organizing tool for creators.

[![CI](https://github.com/EYHN/Anything/actions/workflows/CI.yml/badge.svg)](https://github.com/EYHN/Anything/actions/workflows/CI.yml)
[![codecov](https://codecov.io/gh/EYHN/Anything/branch/develop/graph/badge.svg?token=J9T65C0F2N)](https://codecov.io/gh/EYHN/Anything)

## Features

* Full-featured file manager
* Preview support for text, images, audio, video, fonts, 3D, and more files.
* Powerful metadata mining and search feature.
* Can be installed as a local application or deployed as a cloud service.
* Cross-platform with high performance, using dotnet and web technologies.

## Modules

This repository is a monorepo and contains the following modules.

### anything-core

The backend of `Anything`, running on the `dotnet`.

Status: ![70%](https://img.shields.io/badge/developing-70%25-269539?style=flat-square)


| Name                | Description                                                                                                                                    | Status                                                            |
|---------------------|------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------|
| Anything.FileSystem | Virtual File system abstraction, smoothing out file system differences in between platforms, and also allows for custom file system providers. | ![](https://img.shields.io/badge/-90%25-43853d?style=flat-square) |
| Anything.Database   | Provides storage interface based on sqlite, capable of storing file indexes, image cache, configuration files, etc.                            | ![](https://img.shields.io/badge/-70%25-269539?style=flat-square) |
| Anything.Preview    | Gets file types, icons, thumbnails and metadata from different formats of files.                                                               | ![](https://img.shields.io/badge/-30%25-FF7139?style=flat-square) |
| Anything.Server     | Provides interfaces for clients to access data. Using `graphql` to query data and using `http` to transfer images and files.                   | ![](https://img.shields.io/badge/-30%25-FF7139?style=flat-square) |

### anything-web

A web client for anything, built with `react`.

Status: ![30%](https://img.shields.io/badge/developing-30%25-FF7139?style=flat-square)

## Community

We will release information about project progress, news, and design in [Discussions](https://github.com/EYHN/Anything/discussions).

The official community language is English.

## Contribution

### Ideas

You can post your thoughts and ideas about this project in [Discussions](https://github.com/EYHN/Anything/discussions).

### Develop

#### Back-end

Anything's backend programming language is c#.

You can use `Vistual Studio`, `Vistual Studio Code`, `Github Codespace`, `JetBrains Rider` for development.

The project uses `StyleCop`, and the code needs to pass Roslyn code style analysis.

The project uses `NUnit` and all modules need to cover unit tests. 

#### Front-end

The project uses `npm` as the package manager and uses `lerna` to manage the front-end monorepo.

Clone this repository, run `npm install`, then run `npm run dev` for development.

### Design

We use `figma` for UI and icon design. You can get the latest design [from here](https://www.figma.com/file/tTRMSBrlMmXYjJ82JnBJLn/Anything).