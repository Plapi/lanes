# Changelog

- All notable changes to this package will be documented in this file.
- The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and this package adheres to [Semantic Versioning](https://semver.org/)

## [7.0.7] - 2023-01-10
### Fixed
- Resolved exception that is thrown when inspecting a prefab with the Gradient Effect component.

## [7.0.6] - 2022-11-29
### Fixed
- Sprite atlas works in combination with gradient effect.
- Cleaned up code.

## [7.0.5] - 2022-11-20
### Fixed
- No more error logs when rect transform has a size of zero width or height.
- Maskable toggle is showing properly in inspector.

## [7.0.4] - 2022-10-25
### Fixed
- Shader error when using the asset in combination with a RectMask2D.

## [7.0.3] - 2022-08-18
### Updated
- Adjusted shader to allow for masking of other Graphics.

## [7.0.2] - 2022-06-30
### Fixed
- Bleeding pixels on fill mode.

## [7.0.1] - 2022-05-19
### Removed
- Removed changelog from documentation.

## [7.0.0] - 2022-04-28
### Updated
- Updated dependencies to the latest version.

## [6.2.0] - 2022-03-11
### Added
- Demo scene for gradient effect.

## [6.1.0] - 2022-01-25
### Updated
- Updated dependencies to editor utilities, publishing tools and runtime utilities.

## [6.0.0] - 2022-01-13
### Updated
- Updated dependencies to editor utilities, publishing tools and runtime utilities.

## [5.0.0] - 2021-12-15
### Updated
- Updated editor utilities dependency to 3.0.0
- Updated runtime utilities dependency to 1.1.0

## [4.1.0] - 2021-11-23
### Added
- Added new GradientEffect component

## [4.0.0] - 2021-11-23
### Updated
- Updated editor utilities dependency to 2.0.0
- Updated publishing tools dependency to 1.0.0
- Updated runtime utilities dependency to 1.0.0

## [3.0.4] - 2021-11-19
### Updated
- Update Editor Utilities dependency to a more stable version.

## [2.0.0] - 2021-11-15
### Added
- Added API section
- Add CopyFrom method to copy values from another RoundedImage component.
- Add Reset method to support resetting the component through Unity’s editor options.
- Add SetCornerRounding method overload using ValueTuple<Corner, float>'s as arguments.
- Add ValueEquals method

### Fixed
- Add version define for unity test framework to prevent compile errors from occuring for developers that use visual studio code.

## [1.1.0]
### Added
- Add CopyFrom method to copy values from another RoundedImage component.
- Add Reset method to support resetting the component through Unity’s editor options.
- Add SetCornerRounding method overload using ValueTuple<Corner, float>'s as arguments.
- Add Equals method override