"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { submitApplication } from "@/lib/api";

const US_STATES = [
  "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA","HI","ID","IL","IN","IA",
  "KS","KY","LA","ME","MD","MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ",
  "NM","NY","NC","ND","OH","OK","OR","PA","RI","SC","SD","TN","TX","UT","VT",
  "VA","WA","WV","WI","WY",
];

interface FormData {
  firstName: string;
  lastName: string;
  address: string;
  state: string;
  companyName: string;
  ssn: string;
  requestedAmount: string; // string en el form, se convierte a number al enviar
}

const initialFormData: FormData = {
  firstName: "",
  lastName: "",
  address: "",
  state: "",
  companyName: "",
  ssn: "",
  requestedAmount: "",
};

export default function ApplicationFormPage(){
  const router = useRouter();
  const [formData, setFormData] = useState<FormData>(initialFormData);
  const [errors, setErrors] = useState<Partial<Record<keyof FormData, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null> (null);

  function validate(): boolean {
    const newErrors: Partial<Record<keyof FormData, string>> = {};

    if (!formData.firstName.trim()) newErrors.firstName = "First name is required.";
    if (!formData.lastName.trim()) newErrors.lastName = "Last name is required.";
    if (!formData.address.trim()) newErrors.address = "Address is required.";
    if (!formData.state) newErrors.state = "State is required.";
    if (!formData.companyName.trim()) newErrors.companyName = "Company name is required.";

    // SSN: formato NNN-NN-NNNN 
    // Validación de formato
    if (!/^\d{3}-\d{2}-\d{4}$/.test(formData.ssn)){
      newErrors.ssn = "SSN must be in the format 123-45-6789.";
    }

    const amount = Number(formData.requestedAmount);
    if (!formData.requestedAmount || isNaN(amount) || amount <= 0){
      newErrors.requestedAmount = "Enter a valid amount gretare than 0.";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  async function handleSubmit(e: React.FormEvent){
    e.preventDefault();
    setSubmitError(null);

    if (!validate()) return;

    setIsSubmitting(true);

    try{
      const result = await submitApplication({
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        address: formData.address.trim(),
        state: formData.state,
        companyName: formData.companyName.trim(),
        ssn: formData.ssn,
        requestedAmount: Number(formData.requestedAmount)
      });

      if (result.approved){
        router.push(`/approved?applicationId=${result.applicationId}`);
      }else{
        router.push(`/denied?reason=${encodeURIComponent(result.denialReason ?? "")}`);
      }
    }catch (err){
      // Error de HTTP
      setSubmitError("Something went wrong submitting your application. Please try again.");
    }finally{
      setIsSubmitting(false);
    }
  }

  function handleChange(field: keyof FormData, value: string){
    setFormData((prev) => ({ ...prev, [field]: value }));
  }

  return(
    <main className="mx-auto max-w-lg p-6">
      <h1 className="mb-6 text-2xl font-semibold">Loan Application</h1>

      <form onSubmit={handleSubmit} noValidate className="space-y-4">
        <Field label="First name" error={errors.firstName}>
          <input
            type="text"
            value={formData.firstName}
            onChange={(e) => handleChange("firstName", e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-2"
          />
        </Field>
        <Field label="Last name" error={errors.lastName}>
          <input
            type="text"
            value={formData.lastName}
            onChange={(e) => handleChange("lastName", e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-2"
          />
        </Field>
        <Field label="Address" error={errors.address}>
          <input
            type="text"
            value={formData.address}
            onChange={(e) => handleChange("address", e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-2"
          />
        </Field>
        <Field label="State" error={errors.state}>
          <select
            value={formData.state}
            onChange={(e) => handleChange("state", e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-2"
          >
            <option value="">Select a state</option>
            {US_STATES.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </Field>
        <Field label="Company name" error={errors.companyName}>
          <input
            type="text"
            value={formData.companyName}
            onChange={(e) => handleChange("companyName", e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-2"
          />
        </Field>
        <Field label="SSN" error={errors.ssn}>
          <input
            type="text"
            placeholder="123-45-6789"
            value={formData.ssn}
            onChange={(e) => handleChange("ssn", e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-2"
          />
        </Field>
        <Field label="Requested amount" error={errors.requestedAmount}>
          <input
            type="number"
            min="0"
            step="0.01"
            value={formData.requestedAmount}
            onChange={(e) => handleChange("requestedAmount", e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-2"
          />
        </Field>
        {submitError && (
          <p className="text-sm text-red-600" role="alert">{submitError}</p>
        )}

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded bg-blue-600 px-4 py-2 text-white disabled:opacity-50"
        >
          {isSubmitting ? "Submitting..." : "Submit application"}
        </button>
      </form>
    </main>
  );
}

function Field({
  label,
  error,
  children,
}: {
  label: string;
  error?: string;
  children: React.ReactNode;
}) {
  return(
    <div>
      <label className="mb-1 block text-sm font-medium text-gray-700">{label}</label>
      {children}
      {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
    </div>
  );
}