﻿#ifndef __AFMROUTINES
#define __AFMROUTINES
#if !(defined AUTOMAAT) || (defined VISSIM)
	#include "xyprintf.h"
#endif
#include <stdlib.h>


#define AFM_FC			0
#define AFM_GMAXCCOL	1
#define AFM_MINGMAX		2
#define AFM_MAXGMAX		3
#define AFM_GACT		4
#define AFM_GGEM		5
#define AFM_AFGEKAPT	6
#define AFM_GMAXAFM		7
#define AFM_QLENGHT		8
#define AFM_RELBUFFERRUIMTE		9
#define AFM_RELBUFFERVULLING	10

struct AFM_fc
{
	count fc;			/* CCOL fc index */
	count afm_prm;		/* CCOL AFM prm index */
	mulv gact;			/* Actuele VG tijd */
	mulv gmax_ccol;		/* Maximale VG tijd */
	mulv min_gmax;		/* Minimale maximale VG tijd */
	mulv max_gmax;		/* Minimale maximale VG tijd */
	mulv gmax_gem;		/* Gemiddeld VG tijd over afgelopen 3 cycli */
	mulv gmax_tijd[3];	/* Opslag laatste 3 VG tijden */
	mulv tc_start;		/* Starttijd cyclustijd meting */
	mulv tc_einde_prev;	/* Opslag cyclustijd vorige machineslag */
	mulv tc;			/* Actuele cyclustijd */
	mulv tc_gem;		/* Gemiddelde cyclustijd over de afgelopen 3 cycli */
	mulv tc_tijd[3];	/* Opslag afgelopen 3 cyclustijden */
	mulv afgekapt;	    /* Registreren of richting is afgekapt */
	
	mulv gmax_afm;       /* Door AFM aangevraagde VG percentage */
};
#define AFM_FC_STRUCT struct AFM_fc

#endif