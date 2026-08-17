
"use client";

import { useSearchParams } from "next/navigation";
import Link from "next/link";

export default function DeniedPage() {
  const searchParams = useSearchParams();
  const reason = searchParams.get("reason");

  return (
    <main className="mx-auto max-w-lg p-6 text-center">
      <div className="mb-4 text-5xl">❌</div>
      <h1 className="mb-2 text-2xl font-semibold text-red-700">
        Application Denied
      </h1>
      <p className="mb-6 text-gray-600">
        {reason || "We're unable to approve your application at this time."}
      </p>
      <Link
        href="/"
        className="inline-block rounded bg-blue-600 px-4 py-2 text-white"
      >
        Submit another application
      </Link>
    </main>
  );
}