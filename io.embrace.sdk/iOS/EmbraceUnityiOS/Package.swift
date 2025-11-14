// swift-tools-version: 5.7
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "EmbraceUnityiOS",
    platforms: [
        .iOS(.v13), .tvOS(.v13), .macOS(.v13), .watchOS(.v6)
    ],
    products: [
        .library(name: "EmbraceUnityiOS", targets: ["EmbraceUnityiOS"]),
    ],
    dependencies: [
        .package(url: "https://github.com/embrace-io/embrace-apple-sdk.git", exact: "6.15.0")
    ],
    targets: [
        .target(
            name: "EmbraceUnityiOS",
            dependencies: [
                .product(name: "EmbraceCore", package: "embrace-apple-sdk"),
                .product(name: "EmbraceCrash", package: "embrace-apple-sdk"),
                .product(name: "EmbraceCrashlyticsSupport", package: "embrace-apple-sdk"),
                .product(name: "EmbraceIO", package: "embrace-apple-sdk"),
                .product(name: "EmbraceSemantics", package: "embrace-apple-sdk")
            ]
        ),
        .testTarget(
            name: "EmbraceUnityiOSTests",
            dependencies: ["EmbraceUnityiOS"]
        ),
    ]
)
