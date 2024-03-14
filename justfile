# Runs the formatter for all files
fmt:
    dotnet csharpier .
    
# Initializes git hooks within the repository
prepare:
    git config core.hookspath .githooks
    
# Deploy image to fly
deploy:
    fly deploy
    
# Runs the dev server within the context of the web service
watch:
    just src/VitalTrack.Web/