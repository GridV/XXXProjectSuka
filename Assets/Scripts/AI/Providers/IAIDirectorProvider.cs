using System;

// Simple synchronous provider interface for the AI Director.
public interface IAIDirectorProvider
{
    // Returns an AIDirectorResponse for the director to consume.
    AIDirectorResponse GetAIDirectorResponse();

    // Returns an AIDirectorResponse based on the provided request.
    AIDirectorResponse GetAIDirectorResponse(AIDirectorRequest request);
}
