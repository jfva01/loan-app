"use client";

import { useSearchParams } from "next/navigation";
import Link from "next/link";

export default function ApprovedPage() {
  const searchParams = useSearchParams();
  const applicationId = searchParams.get("applicationId");
  const isReturningCustomer = searchParams.get("returning") === "true";

  return (
    <main className="mx-auto max-w-lg p-6 text-center">
      <div className="mb-4 text-5xl">✅</div>
      <h1 className="mb-2 text-2xl font-semibold text-green-700">
        Application Approved
      </h1>
      <p className="mb-6 text-gray-600">
        {isReturningCustomer
          ? "Your existing application has been updated with your new information."
          : "Your loan application has been approved."
        }
      </p>
      {applicationId && (
        <p className="mb-6 text-sm text-gray-400">
          Application ID: {applicationId}
        </p>
      )}
      <Link
        href="/"
        className="inline-block rounded bg-blue-600 px-4 py-2 text-white"
      >
        Submit another application
      </Link>
    </main>
  );
}