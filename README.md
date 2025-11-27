# AI Book Author Pro

A professional Windows desktop application for AI-powered book creation, designed to streamline the entire writing process from outline to export-ready manuscript.

## Features

### Core Capabilities
- **Multi-Provider AI Integration**: Support for Anthropic Claude, OpenAI GPT, and Google Gemini
- **Project Management**: ZIP-based project files (.abpro) with autosave and recovery
- **Rich Text Editing**: Full WPF RichTextBox with formatting support
- **Chapter Organization**: Drag-and-drop chapter management with scenes
- **Character & World Building**: Track characters, locations, and research notes

### AI-Powered Writing
- Generate complete chapters from outlines
- Expand and rewrite selected text
- Create character profiles and backstories
- Generate plot outlines and story structures
- Context-aware writing that maintains consistency

### Export Options
- Microsoft Word (.docx) with full formatting
- PDF with customizable page layouts
- EPUB for e-readers
- HTML and Markdown
- KDP-ready formatting presets

## Technology Stack

- **.NET 8** - Latest LTS release
- **WPF** - Native Windows UI framework
- **Material Design in XAML** - Modern, beautiful UI
- **CommunityToolkit.Mvvm** - MVVM with source generators
- **OpenXML SDK** - Document generation
- **Serilog** - Structured logging

## Architecture

```
src/
├── AIBookAuthorPro.App/           # WPF Application (entry point)
├── AIBookAuthorPro.Core/          # Domain models, interfaces, enums
├── AIBookAuthorPro.Application/   # Application services, DTOs
├── AIBookAuthorPro.Infrastructure/ # AI providers, file services, exporters
└── AIBookAuthorPro.UI/            # ViewModels, Views, Converters
```

## Getting Started

### Prerequisites
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 or VS Code with C# extension

### Building

```bash
# Clone the repository
git clone https://github.com/ncreighton/AIBookAuthorPro.git
cd AIBookAuthorPro

# Restore packages and build
dotnet restore
dotnet build

# Run the application
dotnet run --project src/AIBookAuthorPro.App
```

### Configuration

Create `appsettings.Development.json` in the App project with your API keys:

```json
{
  "AIProviders": {
    "Anthropic": {
      "ApiKey": "your-anthropic-key"
    },
    "OpenAI": {
      "ApiKey": "your-openai-key"
    },
    "Gemini": {
      "ApiKey": "your-gemini-key"
    }
  }
}
```

Or set environment variables:
```
AIBOOKAUTHOR_AIProviders__Anthropic__ApiKey=your-key
AIBOOKAUTHOR_AIProviders__OpenAI__ApiKey=your-key
AIBOOKAUTHOR_AIProviders__Gemini__ApiKey=your-key
```

## Project Status

🚧 **In Active Development**

### Completed
- [x] Solution structure with Clean Architecture
- [x] Core domain models (Project, Chapter, Character, Outline)
- [x] AI provider interfaces and factory
- [x] Anthropic, OpenAI, and Gemini providers with streaming
- [x] Project service with ZIP-based persistence
- [x] DOCX exporter with full formatting
- [x] Main window with Material Design UI
- [x] Dependency injection configuration

### In Progress
- [ ] Rich text editor with data binding
- [ ] AI generation dialog and progress
- [ ] Chapter/scene management UI
- [ ] Character and location editors

### Planned
- [ ] PDF and EPUB exporters
- [ ] Outline editor with drag-and-drop
- [ ] Research notes integration
- [ ] Writing statistics and goals
- [ ] Spell check and grammar integration
- [ ] Plugin system for custom AI prompts
- [ ] MSIX packaging for distribution

## License

Copyright (c) 2024 Nick Creighton. All rights reserved.

## Author

Nick Creighton - [GitHub](https://github.com/ncreighton)
