using System;

namespace DuckGame;

/// <summary>
/// Fully managed resampler, based on Cockos WDL Resampler
/// </summary>
internal class WdlResampler
{
	private class WDL_Resampler_IIRFilter
	{
		private double m_fpos;

		private double m_a1;

		private double m_a2;

		private double m_b0;

		private double m_b1;

		private double m_b2;

		private double[,] m_hist;

		public WDL_Resampler_IIRFilter()
		{
			m_fpos = -1.0;
			Reset();
		}

		public void Reset()
		{
			m_hist = new double[256, 4];
		}

		public void setParms(double fpos, double Q)
		{
			if (!(Math.Abs(fpos - m_fpos) < 1E-06))
			{
				m_fpos = fpos;
				double num = fpos * Math.PI;
				double cpos = Math.Cos(num);
				double alpha = Math.Sin(num) / (2.0 * Q);
				double sc = 1.0 / (1.0 + alpha);
				m_b1 = (1.0 - cpos) * sc;
				m_b2 = (m_b0 = m_b1 * 0.5);
				m_a1 = -2.0 * cpos * sc;
				m_a2 = (1.0 - alpha) * sc;
			}
		}

		public void Apply(float[] inBuffer, int inIndex, float[] outBuffer, int outIndex, int ns, int span, int w)
		{
			double b0 = m_b0;
			double b1 = m_b1;
			double b2 = m_b2;
			double a1 = m_a1;
			double a2 = m_a2;
			while (ns-- != 0)
			{
				double inx = inBuffer[inIndex];
				inIndex += span;
				double outx = inx * b0 + m_hist[w, 0] * b1 + m_hist[w, 1] * b2 - m_hist[w, 2] * a1 - m_hist[w, 3] * a2;
				m_hist[w, 1] = m_hist[w, 0];
				m_hist[w, 0] = inx;
				m_hist[w, 3] = m_hist[w, 2];
				m_hist[w, 2] = denormal_filter(outx);
				outBuffer[outIndex] = (float)m_hist[w, 2];
				outIndex += span;
			}
		}

		private double denormal_filter(float x)
		{
			return x;
		}

		private double denormal_filter(double x)
		{
			return x;
		}
	}

	private const int WDL_RESAMPLE_MAX_FILTERS = 4;

	private const int WDL_RESAMPLE_MAX_NCH = 64;

	private const double PI = Math.PI;

	private double m_sratein;

	private double m_srateout;

	private double m_fracpos;

	private double m_ratio;

	private double m_filter_ratio;

	private float m_filterq;

	private float m_filterpos;

	private float[] m_rsinbuf;

	private float[] m_filter_coeffs;

	private WDL_Resampler_IIRFilter m_iirfilter;

	private int m_filter_coeffs_size;

	private int m_last_requested;

	private int m_filtlatency;

	private int m_samples_in_rsinbuf;

	private int m_lp_oversize;

	private int m_sincsize;

	private int m_filtercnt;

	private int m_sincoversize;

	private bool m_interp;

	private bool m_feedmode;

	/// <summary>
	/// Creates a new Resampler
	/// </summary>
	public WdlResampler()
	{
		m_filterq = 0.707f;
		m_filterpos = 0.693f;
		m_sincoversize = 0;
		m_lp_oversize = 1;
		m_sincsize = 0;
		m_filtercnt = 1;
		m_interp = true;
		m_feedmode = false;
		m_filter_coeffs_size = 0;
		m_sratein = 44100.0;
		m_srateout = 44100.0;
		m_ratio = 1.0;
		m_filter_ratio = -1.0;
		Reset();
	}

	/// <summary>
	/// sets the mode
	/// if sinc set, it overrides interp or filtercnt
	/// </summary>
	public void SetMode(bool interp, int filtercnt, bool sinc, int sinc_size = 64, int sinc_interpsize = 32)
	{
		m_sincsize = ((sinc && sinc_size >= 4) ? ((sinc_size > 8192) ? 8192 : sinc_size) : 0);
		m_sincoversize = ((m_sincsize == 0) ? 1 : ((sinc_interpsize <= 1) ? 1 : ((sinc_interpsize >= 4096) ? 4096 : sinc_interpsize)));
		m_filtercnt = ((m_sincsize == 0) ? ((filtercnt > 0) ? ((filtercnt >= 4) ? 4 : filtercnt) : 0) : 0);
		m_interp = interp && m_sincsize == 0;
		if (m_sincsize == 0)
		{
			m_filter_coeffs = new float[0];
			m_filter_coeffs_size = 0;
		}
		if (m_filtercnt == 0)
		{
			m_iirfilter = null;
		}
	}

	/// <summary>
	/// Sets the filter parameters
	/// used for filtercnt&gt;0 but not sinc
	/// </summary>
	public void SetFilterParms(float filterpos = 0.693f, float filterq = 0.707f)
	{
		m_filterpos = filterpos;
		m_filterq = filterq;
	}

	/// <summary>
	/// Set feed mode
	/// </summary>
	/// <param name="wantInputDriven">if true, that means the first parameter to ResamplePrepare will specify however much input you have, not how much you want</param>
	public void SetFeedMode(bool wantInputDriven)
	{
		m_feedmode = wantInputDriven;
	}

	/// <summary>
	/// Reset
	/// </summary>
	public void Reset(double fracpos = 0.0)
	{
		m_last_requested = 0;
		m_filtlatency = 0;
		m_fracpos = fracpos;
		m_samples_in_rsinbuf = 0;
		if (m_iirfilter != null)
		{
			m_iirfilter.Reset();
		}
	}

	public void SetRates(double rate_in, double rate_out)
	{
		if (rate_in < 1.0)
		{
			rate_in = 1.0;
		}
		if (rate_out < 1.0)
		{
			rate_out = 1.0;
		}
		if (rate_in != m_sratein || rate_out != m_srateout)
		{
			m_sratein = rate_in;
			m_srateout = rate_out;
			m_ratio = m_sratein / m_srateout;
		}
	}

	public double GetCurrentLatency()
	{
		double v = ((double)m_samples_in_rsinbuf - (double)m_filtlatency) / m_sratein;
		if (v < 0.0)
		{
			v = 0.0;
		}
		return v;
	}

	/// <summary>
	/// Prepare
	/// note that it is safe to call ResamplePrepare without calling ResampleOut (the next call of ResamplePrepare will function as normal)
	/// nb inbuffer was WDL_ResampleSample **, returning a place to put the in buffer, so we return a buffer and offset
	/// </summary>
	/// <param name="out_samples">req_samples is output samples desired if !wantInputDriven, or if wantInputDriven is input samples that we have</param>
	/// <param name="nch"></param>
	/// <param name="inbuffer"></param>
	/// <param name="inbufferOffset"></param>
	/// <returns>returns number of samples desired (put these into *inbuffer)</returns>
	public int ResamplePrepare(int out_samples, int nch, out float[] inbuffer, out int inbufferOffset)
	{
		if (nch > 64 || nch < 1)
		{
			inbuffer = null;
			inbufferOffset = 0;
			return 0;
		}
		int fsize = 0;
		if (m_sincsize > 1)
		{
			fsize = m_sincsize;
		}
		int hfs = fsize / 2;
		if (hfs > 1 && m_samples_in_rsinbuf < hfs - 1)
		{
			m_filtlatency += hfs - 1 - m_samples_in_rsinbuf;
			m_samples_in_rsinbuf = hfs - 1;
			if (m_samples_in_rsinbuf > 0)
			{
				m_rsinbuf = new float[m_samples_in_rsinbuf * nch];
			}
		}
		int sreq = 0;
		sreq = (m_feedmode ? out_samples : ((int)(m_ratio * (double)out_samples) + 4 + fsize - m_samples_in_rsinbuf));
		if (sreq < 0)
		{
			sreq = 0;
		}
		while (true)
		{
			Array.Resize(ref m_rsinbuf, (m_samples_in_rsinbuf + sreq) * nch);
			int sz = m_rsinbuf.Length / ((nch == 0) ? 1 : nch) - m_samples_in_rsinbuf;
			if (sz == sreq)
			{
				break;
			}
			if (sreq > 4 && sz == 0)
			{
				sreq /= 2;
				continue;
			}
			sreq = sz;
			break;
		}
		inbuffer = m_rsinbuf;
		inbufferOffset = m_samples_in_rsinbuf * nch;
		m_last_requested = sreq;
		return sreq;
	}

	public int ResampleOut(float[] outBuffer, int outBufferIndex, int nsamples_in, int nsamples_out, int nch)
	{
		if (nch > 64 || nch < 1)
		{
			return 0;
		}
		if (m_filtercnt > 0 && m_ratio > 1.0 && nsamples_in > 0)
		{
			if (m_iirfilter == null)
			{
				m_iirfilter = new WDL_Resampler_IIRFilter();
			}
			int n = m_filtercnt;
			m_iirfilter.setParms(1.0 / m_ratio * (double)m_filterpos, m_filterq);
			int bufIndex = m_samples_in_rsinbuf * nch;
			int offs = 0;
			for (int x = 0; x < nch; x++)
			{
				for (int a = 0; a < n; a++)
				{
					m_iirfilter.Apply(m_rsinbuf, bufIndex + x, m_rsinbuf, bufIndex + x, nsamples_in, nch, offs++);
				}
			}
		}
		m_samples_in_rsinbuf += Math.Min(nsamples_in, m_last_requested);
		int rsinbuf_availtemp = m_samples_in_rsinbuf;
		if (nsamples_in < m_last_requested)
		{
			int fsize = (m_last_requested - nsamples_in) * 2 + m_sincsize * 2;
			int alloc_size = (m_samples_in_rsinbuf + fsize) * nch;
			Array.Resize(ref m_rsinbuf, alloc_size);
			if (m_rsinbuf.Length == alloc_size)
			{
				Array.Clear(m_rsinbuf, m_samples_in_rsinbuf * nch, fsize * nch);
				rsinbuf_availtemp = m_samples_in_rsinbuf + fsize;
			}
		}
		int ret = 0;
		double srcpos = m_fracpos;
		double drspos = m_ratio;
		int localin = 0;
		int outptr = outBufferIndex;
		int ns = nsamples_out;
		int outlatadj = 0;
		if (m_sincsize != 0)
		{
			if (m_ratio > 1.0)
			{
				BuildLowPass(1.0 / (m_ratio * 1.03));
			}
			else
			{
				BuildLowPass(1.0);
			}
			int filtsz = m_filter_coeffs_size;
			int filtlen = rsinbuf_availtemp - filtsz;
			outlatadj = filtsz / 2 - 1;
			int filter = 0;
			if (nch == 1)
			{
				while (ns-- != 0)
				{
					int ipos = (int)srcpos;
					if (ipos >= filtlen - 1)
					{
						break;
					}
					SincSample1(outBuffer, outptr, m_rsinbuf, localin + ipos, srcpos - (double)ipos, m_filter_coeffs, filter, filtsz);
					outptr++;
					srcpos += drspos;
					ret++;
				}
			}
			else if (nch == 2)
			{
				while (ns-- != 0)
				{
					int ipos2 = (int)srcpos;
					if (ipos2 >= filtlen - 1)
					{
						break;
					}
					SincSample2(outBuffer, outptr, m_rsinbuf, localin + ipos2 * 2, srcpos - (double)ipos2, m_filter_coeffs, filter, filtsz);
					outptr += 2;
					srcpos += drspos;
					ret++;
				}
			}
			else
			{
				while (ns-- != 0)
				{
					int ipos3 = (int)srcpos;
					if (ipos3 >= filtlen - 1)
					{
						break;
					}
					SincSample(outBuffer, outptr, m_rsinbuf, localin + ipos3 * nch, srcpos - (double)ipos3, nch, m_filter_coeffs, filter, filtsz);
					outptr += nch;
					srcpos += drspos;
					ret++;
				}
			}
		}
		else if (!m_interp)
		{
			if (nch == 1)
			{
				while (ns-- != 0)
				{
					int ipos4 = (int)srcpos;
					if (ipos4 >= rsinbuf_availtemp)
					{
						break;
					}
					outBuffer[outptr++] = m_rsinbuf[localin + ipos4];
					srcpos += drspos;
					ret++;
				}
			}
			else if (nch == 2)
			{
				while (ns-- != 0)
				{
					int ipos5 = (int)srcpos;
					if (ipos5 >= rsinbuf_availtemp)
					{
						break;
					}
					ipos5 += ipos5;
					outBuffer[outptr] = m_rsinbuf[localin + ipos5];
					outBuffer[outptr + 1] = m_rsinbuf[localin + ipos5 + 1];
					outptr += 2;
					srcpos += drspos;
					ret++;
				}
			}
			else
			{
				while (ns-- != 0)
				{
					int ipos6 = (int)srcpos;
					if (ipos6 >= rsinbuf_availtemp)
					{
						break;
					}
					Array.Copy(m_rsinbuf, localin + ipos6 * nch, outBuffer, outptr, nch);
					outptr += nch;
					srcpos += drspos;
					ret++;
				}
			}
		}
		else if (nch == 1)
		{
			while (ns-- != 0)
			{
				int ipos7 = (int)srcpos;
				double fracpos = srcpos - (double)ipos7;
				if (ipos7 >= rsinbuf_availtemp - 1)
				{
					break;
				}
				double ifracpos = 1.0 - fracpos;
				int inptr = localin + ipos7;
				outBuffer[outptr++] = (float)((double)m_rsinbuf[inptr] * ifracpos + (double)m_rsinbuf[inptr + 1] * fracpos);
				srcpos += drspos;
				ret++;
			}
		}
		else if (nch == 2)
		{
			while (ns-- != 0)
			{
				int ipos8 = (int)srcpos;
				double fracpos2 = srcpos - (double)ipos8;
				if (ipos8 >= rsinbuf_availtemp - 1)
				{
					break;
				}
				double ifracpos2 = 1.0 - fracpos2;
				int inptr2 = localin + ipos8 * 2;
				outBuffer[outptr] = (float)((double)m_rsinbuf[inptr2] * ifracpos2 + (double)m_rsinbuf[inptr2 + 2] * fracpos2);
				outBuffer[outptr + 1] = (float)((double)m_rsinbuf[inptr2 + 1] * ifracpos2 + (double)m_rsinbuf[inptr2 + 3] * fracpos2);
				outptr += 2;
				srcpos += drspos;
				ret++;
			}
		}
		else
		{
			while (ns-- != 0)
			{
				int ipos9 = (int)srcpos;
				double fracpos3 = srcpos - (double)ipos9;
				if (ipos9 >= rsinbuf_availtemp - 1)
				{
					break;
				}
				double ifracpos3 = 1.0 - fracpos3;
				int ch = nch;
				int inptr3 = localin + ipos9 * nch;
				while (ch-- != 0)
				{
					outBuffer[outptr++] = (float)((double)m_rsinbuf[inptr3] * ifracpos3 + (double)m_rsinbuf[inptr3 + nch] * fracpos3);
					inptr3++;
				}
				srcpos += drspos;
				ret++;
			}
		}
		if (m_filtercnt > 0 && m_ratio < 1.0 && ret > 0)
		{
			if (m_iirfilter == null)
			{
				m_iirfilter = new WDL_Resampler_IIRFilter();
			}
			int n2 = m_filtercnt;
			m_iirfilter.setParms(m_ratio * (double)m_filterpos, m_filterq);
			int offs2 = 0;
			for (int i = 0; i < nch; i++)
			{
				for (int j = 0; j < n2; j++)
				{
					m_iirfilter.Apply(outBuffer, i, outBuffer, i, ret, nch, offs2++);
				}
			}
		}
		if (ret > 0 && rsinbuf_availtemp > m_samples_in_rsinbuf)
		{
			double adj = (srcpos - (double)m_samples_in_rsinbuf + (double)outlatadj) / drspos;
			if (adj > 0.0)
			{
				ret -= (int)(adj + 0.5);
				if (ret < 0)
				{
					ret = 0;
				}
			}
		}
		int isrcpos = (int)srcpos;
		m_fracpos = srcpos - (double)isrcpos;
		m_samples_in_rsinbuf -= isrcpos;
		if (m_samples_in_rsinbuf <= 0)
		{
			m_samples_in_rsinbuf = 0;
		}
		else
		{
			Array.Copy(m_rsinbuf, localin + isrcpos * nch, m_rsinbuf, localin, m_samples_in_rsinbuf * nch);
		}
		return ret;
	}

	private void BuildLowPass(double filtpos)
	{
		int wantsize = m_sincsize;
		int wantinterp = m_sincoversize;
		if (m_filter_ratio == filtpos && m_filter_coeffs_size == wantsize && m_lp_oversize == wantinterp)
		{
			return;
		}
		m_lp_oversize = wantinterp;
		m_filter_ratio = filtpos;
		int allocsize = (wantsize + 1) * m_lp_oversize;
		Array.Resize(ref m_filter_coeffs, allocsize);
		if (m_filter_coeffs.Length == allocsize)
		{
			m_filter_coeffs_size = wantsize;
			int sz = wantsize * m_lp_oversize;
			int hsz = sz / 2;
			double filtpower = 0.0;
			double windowpos = 0.0;
			double dwindowpos = Math.PI * 2.0 / (double)sz;
			double dsincpos = Math.PI / (double)m_lp_oversize * filtpos;
			double sincpos = dsincpos * (double)(-hsz);
			for (int x = -hsz; x < hsz + m_lp_oversize; x++)
			{
				double val = 287.0 / 800.0 - 0.48829 * Math.Cos(windowpos) + 0.14128 * Math.Cos(2.0 * windowpos) - 0.01168 * Math.Cos(6.0 * windowpos);
				if (x != 0)
				{
					val *= Math.Sin(sincpos) / sincpos;
				}
				windowpos += dwindowpos;
				sincpos += dsincpos;
				m_filter_coeffs[hsz + x] = (float)val;
				if (x < hsz)
				{
					filtpower += val;
				}
			}
			filtpower = (double)m_lp_oversize / filtpower;
			for (int x = 0; x < sz + m_lp_oversize; x++)
			{
				m_filter_coeffs[x] = (float)((double)m_filter_coeffs[x] * filtpower);
			}
		}
		else
		{
			m_filter_coeffs_size = 0;
		}
	}

	private void SincSample(float[] outBuffer, int outBufferIndex, float[] inBuffer, int inBufferIndex, double fracpos, int nch, float[] filter, int filterIndex, int filtsz)
	{
		int oversize = m_lp_oversize;
		fracpos *= (double)oversize;
		int ifpos = (int)fracpos;
		filterIndex += oversize - 1 - ifpos;
		fracpos -= (double)ifpos;
		for (int x = 0; x < nch; x++)
		{
			double sum = 0.0;
			double sum2 = 0.0;
			int fptr = filterIndex;
			int iptr = inBufferIndex + x;
			int i = filtsz;
			while (i-- != 0)
			{
				sum += (double)(filter[fptr] * inBuffer[iptr]);
				sum2 += (double)(filter[fptr + 1] * inBuffer[iptr]);
				iptr += nch;
				fptr += oversize;
			}
			outBuffer[outBufferIndex + x] = (float)(sum * fracpos + sum2 * (1.0 - fracpos));
		}
	}

	private void SincSample1(float[] outBuffer, int outBufferIndex, float[] inBuffer, int inBufferIndex, double fracpos, float[] filter, int filterIndex, int filtsz)
	{
		int oversize = m_lp_oversize;
		fracpos *= (double)oversize;
		int ifpos = (int)fracpos;
		filterIndex += oversize - 1 - ifpos;
		fracpos -= (double)ifpos;
		double sum = 0.0;
		double sum2 = 0.0;
		int fptr = filterIndex;
		int iptr = inBufferIndex;
		int i = filtsz;
		while (i-- != 0)
		{
			sum += (double)(filter[fptr] * inBuffer[iptr]);
			sum2 += (double)(filter[fptr + 1] * inBuffer[iptr]);
			iptr++;
			fptr += oversize;
		}
		outBuffer[outBufferIndex] = (float)(sum * fracpos + sum2 * (1.0 - fracpos));
	}

	private void SincSample2(float[] outptr, int outBufferIndex, float[] inBuffer, int inBufferIndex, double fracpos, float[] filter, int filterIndex, int filtsz)
	{
		int oversize = m_lp_oversize;
		fracpos *= (double)oversize;
		int ifpos = (int)fracpos;
		filterIndex += oversize - 1 - ifpos;
		fracpos -= (double)ifpos;
		double sum = 0.0;
		double sum2 = 0.0;
		double sumb = 0.0;
		double sum2b = 0.0;
		int fptr = filterIndex;
		int iptr = inBufferIndex;
		int i = filtsz / 2;
		while (i-- != 0)
		{
			sum += (double)(filter[fptr] * inBuffer[iptr]);
			sum2 += (double)(filter[fptr] * inBuffer[iptr + 1]);
			sumb += (double)(filter[fptr + 1] * inBuffer[iptr]);
			sum2b += (double)(filter[fptr + 1] * inBuffer[iptr + 1]);
			sum += (double)(filter[fptr + oversize] * inBuffer[iptr + 2]);
			sum2 += (double)(filter[fptr + oversize] * inBuffer[iptr + 3]);
			sumb += (double)(filter[fptr + oversize + 1] * inBuffer[iptr + 2]);
			sum2b += (double)(filter[fptr + oversize + 1] * inBuffer[iptr + 3]);
			iptr += 4;
			fptr += oversize * 2;
		}
		outptr[outBufferIndex] = (float)(sum * fracpos + sumb * (1.0 - fracpos));
		outptr[outBufferIndex + 1] = (float)(sum2 * fracpos + sum2b * (1.0 - fracpos));
	}
}
