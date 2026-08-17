import { SubmitApplicationRequest, SubmitApplicationResponse } from "./types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5298";

export async function submitApplication(
    request: SubmitApplicationRequest
): Promise<SubmitApplicationResponse> {
    const response = await fetch(`${API_BASE_URL}/api/applications`,{
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
    });

    if (!response.ok){
        throw new Error(`Request failed with status ${response.status}`);
    }

    return response.json();
}